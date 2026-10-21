using Vinland.Core.Entities;

namespace Vinland.Core.Infrastructure.Channels;

public abstract record HostCommand;

public sealed record PositionSnapshot(
    (Guid,Vector2)[] Positions
) : HostCommand;

