using Godot;
using Vinland.Core.Connectors.Input;
using Vinland.Core.Infrastructure.Channels;
using InputEvent = Vinland.Core.Connectors.Input.InputEvent;

namespace Vinland.Godot.input;

public class GodotInputSource(ChannelHub channelHub) : InputSource(channelHub)
{

    protected override void ReadSnapshot()
    {
        InputSnapshot = InputSnapshot with
        {
            MoveUp = Input.IsActionPressed("move_up"),
            MoveDown = Input.IsActionPressed("move_down"),
            MoveLeft = Input.IsActionPressed("move_left"),
            MoveRight = Input.IsActionPressed("move_right")
        };
    }

    protected override void ReadEvents()
    {
        CheckAction(InputAction.Attack, "attack");
    }

    private void CheckAction(InputAction action, string actionName)
    {
        if (Input.IsActionJustPressed(actionName))
            InputEvents.Enqueue(new InputEvent(action, InputEventType.Pressed));

        if (Input.IsActionJustReleased(actionName))
            InputEvents.Enqueue(new InputEvent(action, InputEventType.Released));
    }
}