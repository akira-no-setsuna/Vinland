using Vector2Aether = nkast.Aether.Physics2D.Common.Vector2;
using Vector2Mono = Microsoft.Xna.Framework.Vector2;

namespace Game.Core;

public static class Utils
{
    private const float PPM = PhysicsScale.PIXELS_PER_METER;
    public static Vector2Mono ToMono(this Vector2Aether v) 
        => new Vector2Mono(v.X * PPM, v.Y  * PPM);

    public static Vector2Aether ToAether(this Vector2Mono v) 
        => new Vector2Aether(v.X / PPM , v.Y / PPM);
}

public static class PhysicsScale
{
    public const float PIXELS_PER_METER = 16f;
}