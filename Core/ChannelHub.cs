using System;
using System.Threading.Channels;
using nkast.Aether.Physics2D.Common;
using Vinland.Core.Input;

namespace Vinland.Core;

public static class ChannelHub
{
    public static readonly Channel<PhysicsUpdate> PhysicsToMain = Channel.CreateUnbounded<PhysicsUpdate>();
    public static readonly Channel<InputCommand> MainToLogic = Channel.CreateUnbounded<InputCommand>();
}

public record PhysicsUpdate(string EntityId, Vector2 Position);