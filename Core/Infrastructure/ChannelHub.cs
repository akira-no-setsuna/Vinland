using System.Threading.Channels;
using nkast.Aether.Physics2D.Common;
using Vinland.Core.Input;

namespace Vinland.Core.Infrastructure;

public class ChannelHub
{
    public readonly Channel<PhysicsUpdate> PhysicsToMain = Channel.CreateBounded<PhysicsUpdate>(
        new BoundedChannelOptions(1) 
    { 
        FullMode = BoundedChannelFullMode.DropOldest 
    });
    
    /// <summary>
    /// You can only write/read <see cref="InputCommand"/>
    /// </summary>
    public readonly Channel<InputCommand> MainToLogic = Channel.CreateBounded<InputCommand>(
        new BoundedChannelOptions(1) 
    { 
        FullMode = BoundedChannelFullMode.DropOldest 
    });
}

public record PhysicsUpdate(string EntityId, Vector2 Position);