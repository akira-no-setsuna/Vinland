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
    
    private ThreadManager _threadManager;
    
    private Simulation _simulation;
    protected ChannelHub ChannelHub;
    private InputSource _inputSource;
    private Clock _clock;
    
    public void Initialize()
    {
        Logger.Configure();
        
        // DI
        _services = Bootstrapper.ConfigureServices(modules);
    
        _threadManager = _services.GetRequiredService<ThreadManager>();
        
        
        _simulation = _services.GetRequiredService<Simulation>();
        ChannelHub = _services.GetRequiredService<ChannelHub>();
        
        _inputSource = _services.GetRequiredService<InputSource>();
        _clock = _services.GetRequiredService<Clock>();
        
        Log.Information("=== Initializing game ===");
        
        _threadManager.Start([_simulation]);
    }

    public void Update(float deltaTime)
    {
        _inputSource.Update();
        
        CommandRead();
        FixedUpdate(deltaTime);
        PositionsUpdate();
    }
    
    private void CommandRead()
    {
        while (ChannelHub.Core.Reader.TryRead(out var cmd))
            OnCommand(cmd);
    }

    private void PositionsUpdate()
    {
        ChannelHub.Host.Writer.TryWrite(PositionSnapshot());
    }

    protected abstract void FixedUpdate(float deltaTime);
    protected abstract void OnCommand(CoreCommand cmd);
    protected abstract PositionSnapshot PositionSnapshot();
}