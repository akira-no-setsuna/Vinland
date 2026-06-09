using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.ObjectPool;
using Serilog;

namespace Vinland.Core.Infrastructure;

public static class DependencyInjection
{
    public static IServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();
        
        var serilogLogger = new LoggerConfiguration()
            .WriteTo.Async(a => a.File("logs/game-.txt", rollingInterval: RollingInterval.Day))
            .Enrich.WithThreadId()
            .MinimumLevel.Debug() 
            .CreateLogger();
        
        Log.Logger = serilogLogger;
        
        services.AddSingleton<Serilog.ILogger>(serilogLogger);
        services.AddSingleton<ChannelHub>();
        services.AddSingleton<ObjectPoolProvider, DefaultObjectPoolProvider>();
        
        return services.BuildServiceProvider();
    }
}