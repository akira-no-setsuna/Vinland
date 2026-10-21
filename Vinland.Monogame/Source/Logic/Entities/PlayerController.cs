using System;
using Vinland.Monogame.Infrastructure.Channels;
using Vinland.Monogame.Infrastructure.Channels.Commands;

namespace Vinland.Monogame.Logic.Entities;

// TODO: Напрягает постоянно писать ChannelHub, решить
public class PlayerController(ChannelHub channelHub)
{
    public void FixedUpdate(long tick, Guid playerID, float speed)
    {
        channelHub.InputSnapshots.Reader.TryRead(out var input);

        var velocity = input.MoveDirection * speed;
        var command = new SetVelocity(tick, playerID, velocity);

        channelHub.LogicToPhysics.Writer.TryWrite(command);
    }
}