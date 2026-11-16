using System.Buffers;
using Serilog;
using Vinland.Core.Connectors.Input;
using Vinland.Core.Infrastructure.Channels;

namespace Vinland.Core.Entities;

public class PlayerController(ChannelHub channelHub,long playerID)
{
    private InputSnapshot _last;

    public void Update(float speed)
    {
        if (channelHub.InputSnapshots.Reader.TryRead(out var input))
            _last = input;
        
        var command = new SetVelocity( playerID, _last.MoveDirection * speed);
        channelHub.Core.Writer.TryWrite(command);
    }
}