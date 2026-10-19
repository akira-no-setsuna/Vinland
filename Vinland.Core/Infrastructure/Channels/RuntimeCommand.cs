namespace Vinland.Core.Infrastructure.Channels;

public abstract record RuntimeCommand;
public sealed record PositionSnapshot(
    Guid[] EntityID,
    Vector2[] Position
) : RuntimeCommand;