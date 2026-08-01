using System.Threading;

namespace Game.Core.Infrastructure;

public static class GameClock
{
    private static long _tick;
    
    public static void Increment() => Interlocked.Increment(ref _tick);
    public static long Current => Interlocked.Read(ref _tick);
}