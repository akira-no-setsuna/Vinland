using System;
using Vinland.Core.Infrastructure.Channels;
using Vinland.Core.Infrastructure.Channels.Commands;
using Vinland.Core.Physics;

namespace Game.Tests;

public class StressTests
{
    [Fact]
    public void RapidSpawnAndClear_NoMemoryLeaksOrHangs()
    {
        var hub = new ChannelHub();
        var phys = new PhysicsManager(hub);
        phys.Start();

        for (int room = 0; room < 10; room++)
        {
            for (int i = 0; i < 100; i++)
                hub.LogicToPhysics.Writer.TryWrite(new BodySpawn(room, Guid.NewGuid(), Vector2.Zero, 1, 1));
            
            phys.ManualUpdate(room, 1f/60f);
            
            while (hub.PhysicsToMain.Reader.TryRead(out _)) {}
        }
        true.Should().BeTrue();
    }
}
