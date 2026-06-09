using System.Threading.Channels;
using nkast.Aether.Physics2D.Common;
using Vinland.Core.Input;

namespace Vinland.Core.Infrastructure;

public class ChannelHub
{
    public readonly Channel<PhysicsUpdate> PhysicsToMain = Channel.CreateUnbounded<PhysicsUpdate>();
    public readonly Channel<InputCommand> MainToLogic = Channel.CreateUnbounded<InputCommand>();
}

public record PhysicsUpdate(string EntityId, Vector2 Position);