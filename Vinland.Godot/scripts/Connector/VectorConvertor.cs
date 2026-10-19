using Vector2GD = Godot.Vector2;
using Vector2Net = System.Numerics.Vector2;

namespace Vinland.Godot.Connector;

public static class VectorConvertor
{
    private const float PIXELS_PER_METER = 16f;
    
    public static Vector2GD ToGodot(this Vector2Net v)
    {
        return new Vector2GD(v.X * PIXELS_PER_METER, v.Y * PIXELS_PER_METER);
    }
    
    public static Vector2Net ToNet(this Vector2GD v)
    {
        return new Vector2Net(v.X * PIXELS_PER_METER, v.Y * PIXELS_PER_METER);
    }
}