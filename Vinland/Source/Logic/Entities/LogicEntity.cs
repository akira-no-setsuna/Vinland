using System;
using Vinland.Core.Data.ConfigClasses;

namespace Vinland.Logic.Entities;

public class LogicEntity(EntityConfig data)
{
    public required Guid Id { get; init; }
    public required EntityKind Kind { get; init; }
    public Vector2 Position { get; set; }

    public float MaxHealth { get; set; } = data.MaxHealth;
    public float Health { get; private set; } = data.MaxHealth;

    public EntityState State { get; set; } = EntityState.Idle;
    public float Speed { get; init; } = data.Speed;

    public bool IsDead { get; private set; }

    public void TakeDamage(float damage)
    {
        Health -= damage;
        IsDead = Health <= 0;
    }
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