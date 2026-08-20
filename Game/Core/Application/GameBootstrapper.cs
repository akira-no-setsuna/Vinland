using System;
using Game.Core.Infrastructure.Channels;
using Game.Core.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.ObjectPool;

namespace Game.Core.Application;

public static class GameBootstrapper
{
    public static IServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();

        services.AddSingleton(GameLogger.Configure(true));
        services.AddSingleton<ChannelHub>();
        services.AddSingleton<ObjectPoolProvider, DefaultObjectPoolProvider>();
        services.AddSingleton<GameThreadManager>();

        return services.BuildServiceProvider();
    }
}