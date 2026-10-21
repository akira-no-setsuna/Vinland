using Microsoft.Extensions.DependencyInjection;
using Vinland.Core.Connectors.Input;
using Vinland.Core.Infrastructure;
using Vinland.Core.Infrastructure.Bootstrap;
using Vinland.Godot.input;

namespace Vinland.Godot.Connector;

public sealed class GameModules : IExternalModules
{
    public void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<InputSource, GodotInputSource>();
    }
}