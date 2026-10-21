
namespace Vinland.Core.Connectors.Input;

public readonly record struct InputSnapshot(
    bool MoveLeft,
    bool MoveRight,
    bool MoveUp,
    bool MoveDown
)
{
    public readonly Vector2 MoveDirection
    {
        get
        {
            Vector2 dir = new(
                (MoveRight ? 1f : 0f) - (MoveLeft ? 1f : 0f),
                (MoveDown ? 1f : 0f) - (MoveUp ? 1f : 0f)
            );
            
            return dir.LengthSquared() > 0 
                ? Vector2.Normalize(dir) 
                : Vector2.Zero;
        }
    }
}