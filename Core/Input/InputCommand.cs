namespace Vinland.Core.Input;
using Vector2Aether = nkast.Aether.Physics2D.Common.Vector2;

public readonly record struct InputCommand(
    bool MoveLeft,
    bool MoveRight,
    bool MoveUp,
    bool MoveDown,
    bool Attack)
{
    public readonly Vector2Aether MoveDirection => new(
        (MoveRight ? 1f : 0f) - (MoveLeft ? 1f : 0f),
        (MoveDown ? 1f : 0f) - (MoveUp ? 1f : 0f)
    );
}

public interface IInputSource
{
    InputCommand ReadInput();
}
