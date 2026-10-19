using MonoGame.Extended.Tilemaps;

namespace Vinland.Monogame.Core.Infrastructure.Channels.Commands;

public abstract record MainCommand(long Tick);

public sealed record GenerateMapColliders(
    long Tick,
    Tilemap Tilemap
) : MainCommand(Tick);