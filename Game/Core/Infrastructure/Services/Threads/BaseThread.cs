using Serilog;

namespace Game.Core.Infrastructure.Services.Threads;

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
        FixedUpdate(tick, deltaTime);
    }

    protected virtual void Prepare()
    {
    }

    protected abstract void FixedUpdate(long tick, float deltaTime);
}