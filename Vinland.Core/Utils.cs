using System.Runtime.CompilerServices;

namespace Vinland.Core;

public static class Utils
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static (int X, int Y) GetCell(this Vector2 position)
    {
        return ((int)MathF.Floor(position.X), (int)MathF.Floor(position.Y));
    }
}

