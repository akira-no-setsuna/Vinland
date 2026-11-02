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

public struct CircleCells
{
    private readonly float _cx, _cy, _r2;
    private readonly int _maxY;
    private int _y, _x, _endX;

    public CircleCells(Vector2 center, float radius)
    {
        _cx = center.X;
        _cy = center.Y;
        _r2 = radius * radius;
        
        _y    = (int)MathF.Floor(_cy - radius) - 1;
        _maxY = (int)MathF.Floor(_cy + radius);
        _x    = 0;
        _endX = -1;
    }

    public CircleCells GetEnumerator() => this;
    public (int X, int Y) Current => (_x, _y);

    public bool MoveNext()
    {
        if (_x < _endX) { _x++; return true; }

        while (_y < _maxY)
        {
            _y++;
            float dy = _cy < _y ? _y - _cy : (_cy > _y + 1 ? _cy - (_y + 1) : 0f);
            float rem = _r2 - dy * dy;
            if (rem < 0f) continue;

            float hw = MathF.Sqrt(rem);
            int sx = (int)MathF.Ceiling(_cx - hw) - 1;
            int ex = (int)MathF.Floor(_cx + hw);
            if (sx > ex) continue;

            _x = sx;
            _endX = ex;
            return true;
        }
        return false;
    }
}

