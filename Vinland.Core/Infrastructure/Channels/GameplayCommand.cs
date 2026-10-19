using Vinland.Core.Entities;

namespace Vinland.Core.Infrastructure.Channels;

public abstract record GameplayCommand;

public sealed record SpawnCommand(
    Guid EntityID,
    EntityKind Kind,
    Vector2 Position
) : GameplayCommand;

public sealed record SetVelocity(
    Guid EntityID,
    Vector2 Velocity
) : GameplayCommand;

