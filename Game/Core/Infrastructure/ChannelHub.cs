using System.Threading.Channels;
using Game.Core.Input;
using Game.Core.Logic;
using Vector2Aether = nkast.Aether.Physics2D.Common.Vector2;
using Vector2Mono = Microsoft.Xna.Framework.Vector2;

namespace Game.Core.Infrastructure;

public class ChannelHub
{
    
    
    /// <summary>
    /// You can only write/read <see cref="PhysicsUpdate"/>
    /// </summary>
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
    
    /// <summary>
    /// You can only write/read <see cref="LogicCommand"/>
    /// </summary>
    public readonly Channel<LogicCommand> LogicToMain = Channel.CreateBounded<LogicCommand>(
        new BoundedChannelOptions(1) 
        { 
            FullMode = BoundedChannelFullMode.DropOldest 
        });
    
    
}

public record PhysicsUpdate
{
    public string EntityId { get; }
    public Vector2Mono Position { get; }
    
    /// <summary>
    /// Creates an update, automatically converting the position from meters to pixels.
    /// </summary>
    /// <param name="entityId">Entity Identifier</param>
    /// <param name="physicsPosition">Positon in meters (from Aether Physics)</param>
    public PhysicsUpdate(string entityId, Vector2Aether physicsPosition)
    {
        EntityId = entityId;
        Position = physicsPosition.ToMono();
    }
}