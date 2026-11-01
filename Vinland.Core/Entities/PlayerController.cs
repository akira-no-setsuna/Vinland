using System.Buffers;
using Serilog;
using Vinland.Core.Infrastructure.Channels;

namespace Vinland.Core.Entities;

public class PlayerController(ChannelHub channelHub, List<long> playerIDs)
{
    private List<long> _playerIDs = playerIDs;
    private List<Vector2> _velocities = new();
    public void Update(float speed)
    {
        channelHub.InputSnapshots.Reader.TryRead(out var input);
        _velocities.Add(input.MoveDirection * speed);
        
        var command = new SetVelocity( _playerIDs, _velocities);
        channelHub.Core.Writer.TryWrite(command);
        
    }
}