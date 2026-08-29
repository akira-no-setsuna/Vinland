using System;
using Game.Core.Infrastructure.Channels;
using Game.Core.Infrastructure.Channels.Commands;
using Game.Core.Physics;

namespace Game.Tests;

public class PhysicsManagerTests
{
    [Fact]
    public void FixedUpdate_ProcessesSpawnAndVelocity()
    {
        var hub = new ChannelHub();
        var phys = new PhysicsManager(hub);
        phys.Start();
        
        var id = Guid.NewGuid();
        hub.LogicToPhysic.Writer.TryWrite(new BodySpawn(1, id, Vector2.Zero, 1, 1));
        hub.LogicToPhysic.Writer.TryWrite(new SetVelocity(1, id, new Vector2(10, 0)));
        
        phys.ManualUpdate(1, 1f / 60f);
        
        hub.PhysicsToMain.Reader.TryRead(out var cmd).Should().BeTrue();
        cmd.Should().BeOfType<PositionsUpdate>();
    }
}
