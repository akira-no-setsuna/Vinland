using System;
using Game.Core.Infrastructure.Channels;
using Game.Core.Infrastructure.Channels.Commands;

namespace Game.Core.Logic.Entities;

// TODO: Напрягает постоянно писать ChannelHub, решить
public class PlayerController(ChannelHub channelHub)
{
    public void FixedUpdate(long tick, Guid playerID, float speed)
    {
        channelHub.InputSnapshots.Reader.TryRead(out var input);

        var velocity = input.MoveDirection * speed;
        var command = new SetVelocity(tick, playerID, velocity);

        channelHub.LogicToPhysic.Writer.TryWrite(command);
    }
}