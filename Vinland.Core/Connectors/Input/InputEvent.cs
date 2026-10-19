namespace Vinland.Core.Connectors.Input;

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