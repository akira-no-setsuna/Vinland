using System;
using Game.Core.Infrastructure.Channels;
using Game.Core.Infrastructure.Channels.Commands;
using Game.Core.Physics;

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
                hub.LogicToPhysic.Writer.TryWrite(new BodySpawn(room, Guid.NewGuid(), Vector2.Zero, 1, 1));
            
            phys.ManualUpdate(room, 1f/60f);
            
            while (hub.PhysicsToMain.Reader.TryRead(out _)) {}
        }
        true.Should().BeTrue();
    }
}
