using System.Threading.Channels;
using MonoGame.Extended.Input;
using Serilog;

namespace Game.Core.Application.Input;

public readonly record struct InputCommand(
    bool MoveLeft,
    bool MoveRight,
    bool MoveUp,
    bool MoveDown,
    bool Attack)
{
    public readonly Vector2 MoveDirection => new(
        (MoveRight ? 1f : 0f) - (MoveLeft ? 1f : 0f),
        (MoveDown ? 1f : 0f) - (MoveUp ? 1f : 0f)
    );
}

public abstract class InputSource(ChannelWriter<InputCommand> writer)
{
    public void Update()
    {
        KeyboardExtended.Update();
        if (!writer.TryWrite(ReadInput()))
         Log.Warning("Failed to write input.");
    }
    
    protected abstract InputCommand ReadInput();
}
