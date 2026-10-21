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

    public Channel<HostCommand> Host { get; } = Channel.CreateBounded<HostCommand>(
        new BoundedChannelOptions(512)
        {
            FullMode = BoundedChannelFullMode.DropOldest
        });
    
    public Channel<CoreCommand> Core { get; } = Channel.CreateBounded<CoreCommand>(
        new BoundedChannelOptions(512)
        {
            FullMode = BoundedChannelFullMode.DropOldest
        });
}