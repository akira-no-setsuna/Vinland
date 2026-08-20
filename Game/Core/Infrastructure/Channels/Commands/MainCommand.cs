using MonoGame.Extended.Tilemaps;

namespace Game.Core.Infrastructure.Channels.Commands;

public abstract record MainCommand
{
}

public sealed record GenerateMapColliders(
    Tilemap Tilemap
) : MainCommand;