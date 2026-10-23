using Vinland.Core.Entities;

namespace Vinland.Core.Infrastructure.Channels;

public abstract record HostCommand;

public sealed record PositionSnapshot(
    List<long> Entities,
    List<Vector2> Positions
) : HostCommand;

