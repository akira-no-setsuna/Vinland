using System;

namespace Game.Core.Infrastructure.Channels.Commands;

public abstract record LogicToMainCommand(long Tick);

public sealed record TextureSpawn(
    long Tick,
    Guid EntityID,
    Vector2 Position,
    string TextureKey
) : LogicToMainCommand(Tick);

public sealed record SetPlayer(
    long Tick,
    Guid EntityID
) : LogicToMainCommand(Tick);

public abstract record LogicToPhysicCommand(long Tick);

public sealed record BodySpawn(
    long Tick,
    Guid EntityID,
    Vector2 Position,
    float Radius,
    float Density
) : LogicToPhysicCommand(Tick);

public sealed record SetVelocity(
    long Tick,
    Guid EntityID,
    Vector2 Velocity
) : LogicToPhysicCommand(Tick);