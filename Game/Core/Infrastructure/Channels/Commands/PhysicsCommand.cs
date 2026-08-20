using System;
using nkast.Aether.Physics2D.Dynamics;

namespace Game.Core.Infrastructure.Channels.Commands;

public abstract record PhysicsCommand
{
}

public sealed record PositionUpdate(
    Guid EntityID,
    Vector2 Position
) : PhysicsCommand;

public sealed record BodyListRender(
    BodyCollection BodyList
) : PhysicsCommand;