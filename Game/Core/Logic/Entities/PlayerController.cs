using System;
using Game.Core.Infrastructure.Channels;
using Game.Core.Infrastructure.Channels.Commands;

namespace Game.Core.Logic.Entities;

// TODO: Напрягает постоянно писать ChannelHub, решить
public class PlayerController(ChannelHub channelHub)
{
    public void FixedUpdate(Guid playerID, float speed)
    {
        channelHub.InputToLogic.Reader.TryRead(out var input);

        var velocity = Vector2.Zero;

        if (input.MoveUp) velocity.Y -= 1;
        if (input.MoveDown) velocity.Y += 1;
        if (input.MoveLeft) velocity.X -= 1;
        if (input.MoveRight) velocity.X += 1;

        if (velocity.LengthSquared() > 0) velocity.Normalize();

        velocity *= speed;
        var command = new SetVelocity(playerID, velocity);

        channelHub.LogicToPhysic.Writer.TryWrite(command);
    }
}