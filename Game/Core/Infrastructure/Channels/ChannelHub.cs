using System.Threading.Channels;
using Game.Core.Infrastructure.Channels.Commands;
using Game.Core.Main.Input;

namespace Game.Core.Infrastructure.Channels;

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

    public Channel<LogicToMainCommand> LogicToMain { get; } = Channel.CreateBounded<LogicToMainCommand>(
        new BoundedChannelOptions(512)
        {
            FullMode = BoundedChannelFullMode.DropOldest
        });
    
    // Logic
    public Channel<LogicToPhysicCommand> LogicToPhysic { get; } = Channel.CreateBounded<LogicToPhysicCommand>(
        new BoundedChannelOptions(512)
        {
            FullMode = BoundedChannelFullMode.DropOldest
        });

    public Channel<PhysicsCommand> PhysicsToMain { get; } = Channel.CreateBounded<PhysicsCommand>(
        new BoundedChannelOptions(512)
        {
            FullMode = BoundedChannelFullMode.DropOldest
        });
    
    // Physics
    public Channel<PhysicsCommand> PhysicsToLogic { get; } = Channel.CreateBounded<PhysicsCommand>(
        new BoundedChannelOptions(512)
        {
            FullMode = BoundedChannelFullMode.DropOldest
        });
    
    // Main
    public Channel<MainCommand> MainToPhysic { get; } = Channel.CreateBounded<MainCommand>(
        new BoundedChannelOptions(512)
        {
            FullMode = BoundedChannelFullMode.DropOldest
        });
    
    // Data
    public Channel<DataCommand> DataToMain { get; } = Channel.CreateBounded<DataCommand>(
        new BoundedChannelOptions(512)
        {
            FullMode = BoundedChannelFullMode.DropOldest
        });
    
    public Channel<DataCommand> DataToLogic { get; } = Channel.CreateBounded<DataCommand>(
        new BoundedChannelOptions(512)
        {
            FullMode = BoundedChannelFullMode.DropOldest
        });
}