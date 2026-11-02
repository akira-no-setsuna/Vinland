using Vinland.Core.Entities;

namespace Vinland.Core.Infrastructure.Channels;

public abstract record CoreCommand;
public sealed record SpawnCommand(
    long? EntityID,
    EntityKind Kind,
    Vector2 Position
) : CoreCommand;

public sealed record SetVelocity(
    List<long> EntityID,
    List<Vector2> Velocity
) : CoreCommand;

public sealed record RayCast(
    long Origin,
    List<long> Targets
);

public sealed record RayCastRequest(
    List<RayCast> Requests
) : CoreCommand;
