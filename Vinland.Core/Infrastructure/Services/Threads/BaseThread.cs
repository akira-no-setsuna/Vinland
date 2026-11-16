using Serilog;

namespace Vinland.Core.Infrastructure.Services.Threads;

public abstract class BaseThread
{
    public abstract string ThreadName { get; }
    
    private bool _isInitialized;

    public void Start()
    {
        Prepare();
        _isInitialized = true;
    }

    public void ManualUpdate(float deltaTime)
    {
        if (!_isInitialized)
        {
            Log.Warning("{Name} thread is not initialized", ThreadName);
            return;
        }
        
        Update(deltaTime);
    }

    protected virtual void Prepare()
    {
    }

    protected abstract void Update(float deltaTime);
}