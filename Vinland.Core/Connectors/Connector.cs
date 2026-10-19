using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Vinland.Core.Connectors.Input;
using Vinland.Core.Infrastructure.Bootstrap;
using Vinland.Core.Infrastructure.Channels;
using Vinland.Core.Infrastructure.Services;
using Vinland.Core.Infrastructure.Services.Threads;

namespace Vinland.Core.Connectors;

public abstract class Connector(IExternalModules modules)
{
    // DI
    
    private IServiceProvider _services;
    
    private Simulation _simulation;
    private ChannelHub _channelHub;
    private ThreadManager _threadManager;
    private InputSource _inputSource;
    private Clock _clock;
    
    public void Initialize()
    {
        // DI
        _services = Bootstrapper.ConfigureServices(modules);
    
        _simulation = _services.GetRequiredService<Simulation>();
        _channelHub = _services.GetRequiredService<ChannelHub>();
        _threadManager = _services.GetRequiredService<ThreadManager>();
        _inputSource = _services.GetRequiredService<InputSource>();
        _clock = _services.GetRequiredService<Clock>();
        
        Log.Information("=== Initializing game ===");
        
        _threadManager.Start();
        
        _simulation.Start();
    }

    public void Update(float deltaTime)
    {
        _inputSource.Update();
        _clock.Tick(deltaTime, (tick, delta) =>
        {
            _simulation.ManualUpdate(tick, delta);
        });
        CommandRead();
    }
    
    private void CommandRead()
    {
        while (_channelHub.Gameplay.Reader.TryRead(out var cmd))
            OnCommand(cmd);
    }
    
    protected abstract void OnCommand(GameplayCommand cmd);
}