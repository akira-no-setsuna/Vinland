namespace Vinland.Core.Entities;

public class Entity
{
    public required Guid Id { get; init; }
    public required EntityKind Kind { get; init; }
    public Vector2 Position { get; set; }
}

public enum EntityKind
{
    Player = 1,
    Enemy = 2
}

public enum EntityState
{
    Idle = 1,
    Roaming = 2,
    Follow = 3
}