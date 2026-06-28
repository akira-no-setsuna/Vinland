using Vector2Aether = nkast.Aether.Physics2D.Common.Vector2;
using Vector2Mono = Microsoft.Xna.Framework.Vector2;

namespace Vinland.Core;

public static class Utils
{
    public static Vector2Mono ToMono(this Vector2Aether v) 
        => new Vector2Mono(v.X, v.Y);

    public static Vector2Aether ToAether(this Vector2Mono v) 
        => new Vector2Aether(v.X, v.Y);
}