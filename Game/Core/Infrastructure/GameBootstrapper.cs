using System;
using Game.Core.Data;
using Game.Core.Infrastructure.Channels;
using Game.Core.Infrastructure.Services;
using Game.Core.Infrastructure.Services.Threads;
using Game.Core.Logic;
using Game.Core.Main.Input;
using Game.Core.Physics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.ObjectPool;

namespace Game.Core.Infrastructure;

public static class GameBootstrapper
{
    public static IServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();

        services.AddSingleton<ChannelHub>();
        services.AddSingleton<ObjectPoolProvider, DefaultObjectPoolProvider>();
        services.AddSingleton<GameThreadManager>();
        
        services.AddSingleton(sp => new LogicManager
            (sp.GetRequiredService<ChannelHub>()));
        services.AddSingleton(sp => new PhysicsManager
            (sp.GetRequiredService<ChannelHub>()));
        services.AddSingleton(sp => new DataManager
            (sp.GetRequiredService<ChannelHub>()));
        
        services.AddSingleton<GameClock>();
        services.AddSingleton(GameLogger.Configure(true));
        services.AddSingleton<InputSource>(sp => new KbmInputSource
            (sp.GetRequiredService<ChannelHub>()));
        return services.BuildServiceProvider();
    }
}