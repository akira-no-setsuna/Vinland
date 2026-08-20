using System;
using Game.Core.Logic.Entities.Data;

namespace Game.Core.Infrastructure.Channels.Commands;

public abstract record LogicToMainCommand
{
    // public required long Tick { get; init; }
}

public sealed record TextureSpawn(
    Guid EntityID,
    Vector2 Position,
    EntityData EntityData
) : LogicToMainCommand;

public sealed record SetPlayer(
    Guid EntityID
) : LogicToMainCommand;

public abstract record LogicToPhysicCommand
{
    // public required long Tick { get; init; }
}

public sealed record BodySpawn(
    Guid EntityID,
    Vector2 Position,
    EntityData EntityData
) : LogicToPhysicCommand;

public sealed record SetVelocity(
    Guid EntityID,
    Vector2 Velocity
) : LogicToPhysicCommand;