using Serilog;

namespace Game.Core.Infrastructure.Services.Threads;

public abstract class BaseThread
{
    // Fixed Update
    private const float FIXED_DELTA_TIME = 1f / 60f;
    private float _accumulator;
    private bool _isPrepared;

    public void Start()
    {
        Prepare();
        _isPrepared = true;
    }
    
    public void Update(float deltaTime)
    {
        if (!_isPrepared)
        {
            Log.Warning("This thread is not prepared");
            Prepare();
            _isPrepared = true;
        }
        
        
        _accumulator += deltaTime;
        while (_accumulator >= FIXED_DELTA_TIME)
        {
            FixedUpdate(FIXED_DELTA_TIME);
            _accumulator -= FIXED_DELTA_TIME;
        }
    }

    protected abstract void Prepare();
    protected abstract void FixedUpdate(float deltaTime);
}