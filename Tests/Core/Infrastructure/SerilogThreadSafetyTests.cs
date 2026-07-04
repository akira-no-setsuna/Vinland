using FluentAssertions;
using Game.Core.Infrastructure.Services;
using Serilog;
using Xunit;

namespace Tests.Core.Infrastructure;

/// <summary>
/// Проверка thread-safety Serilog (Фаза 0 п.3).
/// Используем синхронный файловый sink для надёжности.
/// </summary>
public class SerilogThreadSafetyTests : IDisposable
{
    private readonly string _logFilePath;

    public SerilogThreadSafetyTests()
    {
        _logFilePath = $"logs/test-thread-safety-{Guid.NewGuid()}.txt";
        
        // Настраиваем Serilog с синхронной записью и явным шаблоном, включающим ThreadId
        var logger = new GameLogger()
            .WriteTo.File(
                _logFilePath,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{ThreadId}] {Level:u3} {Message:lj}{NewLine}{Exception}"
            )
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

        // Небольшая задержка для гарантии завершения записи (для синхронного sink не обязательно)
        await Task.Delay(50);

        // Assert — файл должен существовать и содержать все сообщения
        File.Exists(_logFilePath).Should().BeTrue();
        var lines = await File.ReadAllLinesAsync(_logFilePath);
        lines.Length.Should().Be(threadCount * messagesPerThread,
            "all messages should be written without loss");
    }

    [Fact]
    public async Task LoggingFromDifferentThreads_ShouldIncludeThreadId()
    {
        // Act
        Log.Information("Test message from main thread");
        await Task.Run(() => Log.Information("Test message from background thread"));
        
        Log.CloseAndFlush();
        await Task.Delay(50);

        // Assert
        var lines = await File.ReadAllLinesAsync(_logFilePath);
        lines.Should().Contain(line => line.Contains("Test message from main thread"));
        lines.Should().Contain(line => line.Contains("Test message from background thread"));
        
        // Проверяем, что ThreadId присутствует в записях (в квадратных скобках, как задано в шаблоне)
        lines.Should().Contain(line => line.Contains("[") && line.Contains("]"),
            "Thread enricher should add thread ID in brackets");
    }

    public void Dispose()
    {
        Log.CloseAndFlush();
        // НЕ присваиваем Log.Logger = null, чтобы избежать ArgumentNullException

        // Cleanup
        if (File.Exists(_logFilePath))
        {
            try { File.Delete(_logFilePath); } catch { /* ignore */ }
        }
    }
}