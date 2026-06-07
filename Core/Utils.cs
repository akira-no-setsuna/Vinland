using Vinland.Core.Input;
using Vector2Aither = nkast.Aether.Physics2D.Common.Vector2;
using Vector2Mono = Microsoft.Xna.Framework.Vector2;

namespace Vinland.Core;

public static class Utils
{
    public static Vector2Mono ToMono(this Vector2Aither v) 
        => new Vector2Mono(v.X, v.Y);

    public static Vector2Aither ToAether(this Vector2Mono v) 
        => new Vector2Aither(v.X, v.Y);
}