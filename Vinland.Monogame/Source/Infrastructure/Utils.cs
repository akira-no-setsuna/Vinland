namespace Vinland.Monogame.Infrastructure;

public static class Utils
{
    public const float PIXELS_PER_METER = 16f;

    public static Vector2 ToWorld(this Vector2 v)
    {
        return new Vector2(v.X / PIXELS_PER_METER, v.Y / PIXELS_PER_METER);
    }

    public static Vector2 ToScreen(this Vector2 v)
    {
        return new Vector2(v.X * PIXELS_PER_METER, v.Y * PIXELS_PER_METER);
    }
}