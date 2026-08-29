using System;
using System.Globalization;
using System.Threading;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace Game.Core.Infrastructure.Services;

public static class GameLogger
{
    private const string LOG_FILE_TEMPLATE = "logs/game-.log";

    public static ILogger Configure(bool isDebug)
    {
        var minimumLevel = isDebug ? LogEventLevel.Debug : LogEventLevel.Information;
        
        var logger = new LoggerConfiguration()
            .MinimumLevel.Is(minimumLevel)
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("System", LogEventLevel.Warning)

            // Enrichers
            .Enrich.WithThreadId()
            .Enrich.FromLogContext()
            .Enrich.With<ThreadNameEnricher>()
            // .Enrich.With<GameTickEnricher>()

            // Main Log
            .WriteTo.Async(a => a.File(
                LOG_FILE_TEMPLATE,
                formatProvider: CultureInfo.InvariantCulture,
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7,
                fileSizeLimitBytes: 10 * 1024 * 1024, // 10 MB
                shared: false,
                outputTemplate:
                "[{Timestamp:HH:mm:ss.fff} {Level:u3} Tick:{GameTick} [{ThreadName}]: {Message:lj}{NewLine}{Exception}"
            ))

            // Error Log
            .WriteTo.Logger(lc => lc
                .Filter.ByIncludingOnly(e => e.Level == LogEventLevel.Error || e.Level == LogEventLevel.Fatal)
                .WriteTo.File("logs/errors-.txt", rollingInterval: RollingInterval.Day))
            .CreateLogger();

        // Global logger
        Log.Logger = logger;
        return logger;
    }

    public static void FlushAndClose()
    {
        Log.CloseAndFlush();
    }
}

// Custom Enricher for thread name
public class ThreadNameEnricher : ILogEventEnricher
{
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var name = Thread.CurrentThread.Name ?? $"Thread-{Environment.CurrentManagedThreadId}";
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("ThreadName", name));
    }
}