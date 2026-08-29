namespace Game.Core.Main.Input;

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
            if (dir.LengthSquared() > 0) dir.Normalize();
            return dir;
        }
    }
}

public readonly record struct InputEvent(
    InputAction Action,
    InputEventType Type
);

public enum InputAction : byte
{
    Attack = 1,
    Dodge = 2,
    Pause = 3
}

public enum InputEventType : byte
{
    Pressed = 1,
    Released = 2
}