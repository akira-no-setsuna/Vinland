namespace Vinland.Core.Entities;

public class Entity
{
    public required long ID { get; init; }
    public required Vector2 Position { get; set; }
    public EntityKind Kind { get; set; }
    
    public float Speed { get; set; }
    public float VisionRange { get; set; }
    public List<long> VisibleEntities { get; } = new();
    public long? Target { get; set; }
    public EntityState State { get; set; } =  EntityState.Idle;
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



public abstract record EntityDefinition(
    string Species,
    string Role, 
    string Faction
);

public sealed record Player() : EntityDefinition(
    "human",
    "knight",
    "player"
    );
    
public sealed record Enemy() : EntityDefinition(
    "skeleton",
    "meat",
    "undead"
    );