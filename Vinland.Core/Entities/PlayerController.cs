using Serilog;
using Vinland.Core.Infrastructure.Channels;

namespace Vinland.Core.Entities;

public class PlayerController(ChannelHub channelHub)
{
    public void Update(Guid playerID, float speed)
    {
        channelHub.InputSnapshots.Reader.TryRead(out var input);

        var velocity = input.MoveDirection * speed;
        var command = new SetVelocity( playerID, velocity);
        
        channelHub.Core.Writer.TryWrite(command);
    }
}