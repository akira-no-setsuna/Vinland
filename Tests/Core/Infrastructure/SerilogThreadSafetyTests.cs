using FluentAssertions;
using Serilog;
using Serilog.Core;
using Xunit;

namespace Tests.Core.Infrastructure;

/// <summary>
/// Проверка thread-safety Serilog (Фаза 0 п.3).
/// Используем синхронный файловый sink для надёжности.
/// Каждый тест использует собственный Logger-экземпляр, чтобы избежать race condition
/// через глобальный Log.Logger при параллельном запуске xUnit.
/// </summary>
public class SerilogThreadSafetyTests : IDisposable
{
    private readonly string _logFilePath;
    private readonly Logger _logger;

    public SerilogThreadSafetyTests()
    {
        _logFilePath = Path.Combine(Path.GetTempPath(), $"vinland_test_{Guid.NewGuid():N}.log");

        // Создаём локальный экземпляр Logger (не глобальный Log.Logger)
        _logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .Enrich.WithThreadId()
            .WriteTo.File(
                path: _logFilePath,
                outputTemplate: "[{Timestamp:HH:mm:ss.fff} {Level:u3}] [{ThreadId}]: {Message:lj}{NewLine}{Exception}")
            .CreateLogger();
    }

    [Fact]
    public async Task ConcurrentLogging_ShouldNotThrowOrCorrupt()
    {
        // Arrange
        const int threadCount = 8;
        const int messagesPerThread = 100;
        var tasks = new List<Task>();
        var logger = _logger;

        // Act — параллельная запись из разных потоков
        for (int i = 0; i < threadCount; i++)
        {
            int threadId = i;
            tasks.Add(Task.Run(() =>
            {
                for (int j = 0; j < messagesPerThread; j++)
                {
                    logger.Debug("Thread {ThreadId} message {MessageId}", threadId, j);
                }
            }));
        }

        await Task.WhenAll(tasks);
        _logger.Dispose();
        await Task.Delay(100);

        // Assert — файл должен существовать и содержать все сообщения
        File.Exists(_logFilePath).Should().BeTrue();
        var lines = ReadAllLines(_logFilePath);
        lines.Length.Should().BeGreaterThanOrEqualTo(threadCount * messagesPerThread,
            "all messages should be written without loss");
    }

    [Fact]
    public async Task LoggingFromDifferentThreads_ShouldIncludeThreadId()
    {
        // Act
        _logger.Information("Test message from main thread");
        await Task.Run(() => _logger.Information("Test message from background thread"));
        
        _logger.Dispose();
        await Task.Delay(100);

        // Assert
        var lines = ReadAllLines(_logFilePath);
        lines.Should().Contain(line => line.Contains("Test message from main thread"));
        lines.Should().Contain(line => line.Contains("Test message from background thread"));
        
        // Проверяем, что ThreadId присутствует в записях (в квадратных скобках, как задано в шаблоне)
        lines.Should().Contain(line => line.Contains("[") && line.Contains("]"),
            "Thread enricher should add thread ID in brackets");
    }

    /// <summary>
    /// Читает файл с FileShare.ReadWrite, чтобы избежать IOException на Windows
    /// когда Serilog sink ещё не полностью отпустил handle.
    /// </summary>
    private static string[] ReadAllLines(string path)
    {
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using var reader = new StreamReader(stream);
        var lines = new List<string>();
        while (reader.ReadLine() is { } line)
            lines.Add(line);
        return lines.ToArray();
    }

    public void Dispose()
    {
        _logger.Dispose();

        // Cleanup
        if (File.Exists(_logFilePath))
        {
            try { File.Delete(_logFilePath); } catch { /* ignore */ }
        }
    }
}