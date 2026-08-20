using System.Threading.Channels;
using Game.Core.Application.Input;
using Game.Core.Infrastructure.Channels.Commands;

namespace Game.Core.Infrastructure.Channels;

public class ChannelHub
{
    public Channel<InputCommand> InputToLogic { get; } = Channel.CreateBounded<InputCommand>(
        new BoundedChannelOptions(512)
        {
            FullMode = BoundedChannelFullMode.DropOldest
        });

    public Channel<LogicToMainCommand> LogicToMain { get; } = Channel.CreateBounded<LogicToMainCommand>(
        new BoundedChannelOptions(512)
        {
            FullMode = BoundedChannelFullMode.DropOldest
        });

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

    public Channel<PhysicsCommand> PhysicsToLogic { get; } = Channel.CreateBounded<PhysicsCommand>(
        new BoundedChannelOptions(512)
        {
            FullMode = BoundedChannelFullMode.DropOldest
        });

    public Channel<MainCommand> MainToPhysic { get; } = Channel.CreateBounded<MainCommand>(
        new BoundedChannelOptions(512)
        {
            FullMode = BoundedChannelFullMode.DropOldest
        });
}