using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.ObjectPool;
using Serilog;
using Vinland.Core.Connectors.Input;
using Vinland.Core.Infrastructure.Channels;
using Vinland.Core.Infrastructure.Services;
using Vinland.Core.Infrastructure.Services.Threads;

namespace Vinland.Core.Infrastructure.Bootstrap;

public static class Bootstrapper
{
    public static IServiceCollection RegisterCoreServices(IServiceCollection services)
    {
        services.AddSingleton<ChannelHub>();
        services.AddSingleton<ObjectPoolProvider, DefaultObjectPoolProvider>();
        services.AddSingleton<ThreadManager>();

        services.AddSingleton(sp => new Simulation
            (sp.GetRequiredService<ChannelHub>()));
        
        services.AddSingleton<Clock>();
        services.AddSingleton(Logger.Configure(true));
        

        return services;
    }

    private static class RuntimeContract
    {
        public static readonly Type[] Required =
        [
            typeof(InputSource),
        ];
    }

    public static IServiceProvider ConfigureServices(IExternalModules modules)
    {
        var services = new ServiceCollection();
        RegisterCoreServices(services);
        modules?.RegisterServices(services);

        var registered = services.Select(d => d.ServiceType).ToHashSet();
        var missing = RuntimeContract.Required.Where(t => !registered.Contains(t)).ToArray();

        if (missing.Length > 0)
            Log.Error("Host не зарегистрировал обязательные сервисы: {@MissingServices}", 
                missing.Select(t => t.Name));

        return services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateOnBuild = true,
            ValidateScopes = true,
        });
    }
}