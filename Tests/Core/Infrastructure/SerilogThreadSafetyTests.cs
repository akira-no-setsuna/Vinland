using FluentAssertions;
using Serilog;
using Xunit;

namespace Tests.Core.Infrastructure;

/// <summary>
/// Проверка thread-safety Serilog (Фаза 0 п.3).
/// Async sink + File sink должны корректно работать из разных потоков.
/// </summary>
public class SerilogThreadSafetyTests : IDisposable
{
    private readonly string _logFilePath;

    public SerilogThreadSafetyTests()
    {
        _logFilePath = $"logs/test-thread-safety-{Guid.NewGuid()}.txt";
        
        // Настраиваем Serilog как в DependencyInjection
        var logger = new LoggerConfiguration()
            .WriteTo.Async(a => a.File(_logFilePath))
            .Enrich.WithThreadId()
            .MinimumLevel.Debug()
            .CreateLogger();

        Log.Logger = logger;
    }

    [Fact]
    public async Task ConcurrentLogging_ShouldNotThrowOrCorrupt()
    {
        // Arrange
        const int threadCount = 8;
        const int messagesPerThread = 100;
        var tasks = new List<Task>();

        // Act — параллельная запись из разных потоков
        for (int i = 0; i < threadCount; i++)
        {
            int threadId = i;
            tasks.Add(Task.Run(() =>
            {
                for (int j = 0; j < messagesPerThread; j++)
                {
                    Log.Debug("Thread {ThreadId} message {MessageId}", threadId, j);
                }
            }));
        }

        await Task.WhenAll(tasks);
        Log.CloseAndFlush();

        // Assert — файл должен существовать и содержать сообщения
        File.Exists(_logFilePath).Should().BeTrue();
        var lines = await File.ReadAllLinesAsync(_logFilePath);
        lines.Length.Should().Be(threadCount * messagesPerThread,
            "all messages should be written without loss");
    }

    [Fact]
    public void LoggingFromDifferentThreads_ShouldIncludeThreadId()
    {
        // Act
        Log.Information("Test message from main thread");
        
        Task.Run(() => Log.Information("Test message from background thread")).Wait();
        
        Log.CloseAndFlush();

        // Assert
        var lines = File.ReadAllLines(_logFilePath);
        lines.Should().Contain(line => line.Contains("Test message from main thread"));
        lines.Should().Contain(line => line.Contains("Test message from background thread"));
        
        // Thread ID enricher должен добавить информацию о потоке
        lines.Should().Contain(line => line.Contains("[") && line.Contains("]"),
            "Thread enricher should add thread ID in brackets");
    }

    public void Dispose()
    {
        Log.CloseAndFlush();
        Log.Logger = null;
        
        // Cleanup
        if (File.Exists(_logFilePath))
        {
            try { File.Delete(_logFilePath); } catch { /* ignore */ }
        }
    }
}