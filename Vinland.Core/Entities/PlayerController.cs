using Serilog;
using Vinland.Core.Infrastructure.Channels;

namespace Vinland.Core.Entities;

public class PlayerController(ChannelHub channelHub)
{
    public void FixedUpdate(Guid playerID, float speed)
    {
        channelHub.InputSnapshots.Reader.TryRead(out var input);

        var velocity = input.MoveDirection * speed;
        var command = new SetVelocity( playerID, velocity);
        
        channelHub.Gameplay.Writer.TryWrite(command);
    }
}