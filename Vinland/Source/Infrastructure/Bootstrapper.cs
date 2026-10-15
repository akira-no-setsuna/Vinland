using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.ObjectPool;
using Vinland.Core.Data;
using Vinland.Core.Infrastructure.Channels;
using Vinland.Core.Infrastructure.Services;
using Vinland.Core.Infrastructure.Services.Threads;
using Vinland.Core.Logic;
using Vinland.Core.Main.Input;
using Vinland.Core.Physics;

namespace Vinland.Core.Infrastructure;

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