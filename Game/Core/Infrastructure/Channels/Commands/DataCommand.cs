namespace Game.Core.Infrastructure.Channels.Commands;

public abstract record DataCommand
{
    // public required long Tick { get; init; }
}

public sealed record TextureLoad(
    string TextureKey
) : DataCommand;