using Vinland.Core.Entities;

namespace Vinland.Core.Infrastructure.Channels;

public abstract record CoreCommand;
public sealed record SpawnCommand(
    Guid EntityID,
    EntityKind Kind,
    Vector2 Position
) : CoreCommand;

public sealed record SetVelocity(
    Guid EntityID,
    Vector2 Velocity
) : CoreCommand;