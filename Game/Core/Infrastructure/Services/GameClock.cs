using System;
using Serilog.Context;

namespace Game.Core.Infrastructure.Services;

public sealed class GameClock
{
    // Fixed Update
    private const float FIXED_DELTA_TIME = 1f / 60f;
    private const float MAX_ACCUMULATOR = 0.25f;
    private float _accumulator;

    public long CurrentTick { get; private set; }


    public void Tick(float deltaTime, Action<long, float> fixedUpdate)
    {
        deltaTime = Math.Clamp(deltaTime, 0f, MAX_ACCUMULATOR);

        _accumulator += deltaTime;
        while (_accumulator >= FIXED_DELTA_TIME)
        {
            CurrentTick++;

            using (LogContext.PushProperty("GameTick", CurrentTick))
            {
                fixedUpdate(CurrentTick, FIXED_DELTA_TIME);
            }

            _accumulator -= FIXED_DELTA_TIME;
        }
    }
}