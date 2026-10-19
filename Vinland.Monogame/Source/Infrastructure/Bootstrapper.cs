using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.ObjectPool;
using Vinland.Monogame.Data;
using Vinland.Monogame.Infrastructure.Channels;
using Vinland.Monogame.Infrastructure.Services;
using Vinland.Monogame.Infrastructure.Services.Threads;
using Vinland.Monogame.Logic;
using Vinland.Monogame.Main.Input;
using Vinland.Monogame.Physics;

namespace Vinland.Monogame.Infrastructure;

public static class Bootstrapper
{
    public static IServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();

        services.AddSingleton<ChannelHub>();
        services.AddSingleton<ObjectPoolProvider, DefaultObjectPoolProvider>();
        services.AddSingleton<ThreadManager>();

        services.AddSingleton(sp => new LogicManager
            (sp.GetRequiredService<ChannelHub>()));
        services.AddSingleton(sp => new PhysicsManager
            (sp.GetRequiredService<ChannelHub>()));
        services.AddSingleton(sp => new DataManager
            (sp.GetRequiredService<ChannelHub>()));

        services.AddSingleton<Clock>();
        services.AddSingleton(Logger.Configure(true));
        services.AddSingleton<InputSource>(sp => new KbmInputSource
            (sp.GetRequiredService<ChannelHub>()));
        return services.BuildServiceProvider();
    }
}