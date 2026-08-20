namespace Game.Core.Logic.Entities.Data;
// TODO: Placeholder, change to JSON format
public abstract record EntityData(
    string Name,
    
    float MaxHealth,
    float Speed,
    
    float Radius,
    float Density,
    
    string TextureKey
    );

public sealed record HumanData() : EntityData(
    "Human",
    100f,
    5f,
    1f,
    1f,
    "textures/Player");
    
public sealed record EnemyData() : EntityData(
    "Enemy",
    50f,
    4f,
    1f,
    1f,
    "textures/Player");