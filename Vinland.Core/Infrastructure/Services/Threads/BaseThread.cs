using Serilog;

namespace Vinland.Core.Infrastructure.Services.Threads;

public abstract class BaseThread
{
    private bool _isInitialized;
    public long CurrentTick { get; private set; }

    public void Start()
    {
        Prepare();
        _isInitialized = true;
    }

    public void ManualUpdate(long tick, float deltaTime)
    {
        if (!_isInitialized)
        {
            Log.Warning("This thread is not initialized");
            return;
        }

        CurrentTick = tick;
        Update(tick, deltaTime);
    }

    protected virtual void Prepare()
    {
    }

    protected abstract void Update(long tick, float deltaTime);
}