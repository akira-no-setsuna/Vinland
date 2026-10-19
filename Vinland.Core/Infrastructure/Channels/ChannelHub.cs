using System.Threading.Channels;
using Vinland.Core.Connectors.Input;

namespace Vinland.Core.Infrastructure.Channels;

public class ChannelHub
{
    // Input
    public Channel<InputSnapshot> InputSnapshots { get; } = Channel.CreateBounded<InputSnapshot>(
        new BoundedChannelOptions(1)
        {
            FullMode = BoundedChannelFullMode.DropOldest,
            SingleReader = true,
            SingleWriter = true
        });

    public Channel<InputEvent> InputEvents { get; } = Channel.CreateBounded<InputEvent>(
        new BoundedChannelOptions(64)
        {
            FullMode = BoundedChannelFullMode.DropOldest,
            SingleReader = true,
            SingleWriter = true
        });

    public Channel<GameplayCommand> Gameplay { get; } = Channel.CreateBounded<GameplayCommand>(
        new BoundedChannelOptions(512)
        {
            FullMode = BoundedChannelFullMode.DropOldest
        });
    
    public Channel<RuntimeCommand> Runtime { get; } = Channel.CreateBounded<RuntimeCommand>(
        new BoundedChannelOptions(512)
        {
            FullMode = BoundedChannelFullMode.DropOldest
        });
}