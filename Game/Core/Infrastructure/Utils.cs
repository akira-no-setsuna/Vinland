using System;
using Game.Core.Data.ConfigClasses;
using Microsoft.Xna.Framework.Graphics;

namespace Game.Core.Infrastructure;

public static class Utils
{
    private const float PPM = PhysicsScale.PIXELS_PER_METER;

    public static Vector2 ToWorld(this Vector2 v)
    {
        return new Vector2(v.X / PPM, v.Y / PPM);
    }

    public static Vector2 ToScreen(this Vector2 v)
    {
        return new Vector2(v.X * PPM, v.Y * PPM);
    }
}

public static class PhysicsScale
{
    public const float PIXELS_PER_METER = 16f;
}

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

public class VisualEntity
{
    public required Guid Id { get; init; }
    public Vector2 Position { get; set; }
    public Texture2D Texture { get; set; }
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