using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using MonoGame.Extended.Tilemaps;
using nkast.Aether.Physics2D.Dynamics;

namespace Game.Core.Infrastructure.Channels.Commands;

public abstract record PhysicsCommand(long Tick);

public sealed record PositionsUpdate(
    long Tick,
    IReadOnlyList<PositionSnapshot> Positions
) : PhysicsCommand(Tick);

// Оставляем короткий вид для record struct, так как у него нет наследования
public readonly record struct PositionSnapshot(
    Guid EntityID,
    Vector2 Position
);

public sealed record BodyListRender(
    long Tick,
    BodyCollection BodyList
) : PhysicsCommand(Tick);