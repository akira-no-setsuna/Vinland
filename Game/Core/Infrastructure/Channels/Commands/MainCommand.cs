using MonoGame.Extended.Tilemaps;

namespace Game.Core.Infrastructure.Channels.Commands;

public abstract record MainCommand(long Tick);

public sealed record GenerateMapColliders(
    long Tick,
    Tilemap Tilemap
) : MainCommand(Tick);