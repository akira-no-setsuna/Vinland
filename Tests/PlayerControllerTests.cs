using System;
using Game.Core.Infrastructure.Channels;
using Game.Core.Infrastructure.Channels.Commands;
using Game.Core.Main.Input;
using Game.Core.Logic.Entities;

namespace Game.Tests;

public class PlayerControllerTests
{
    [Fact]
    public void FixedUpdate_WithInput_SendsVelocity()
    {
        var hub = new ChannelHub();
        var ctrl = new PlayerController(hub);
        var pid = Guid.NewGuid();
        hub.InputSnapshots.Writer.TryWrite(new InputSnapshot(false, true, false, false));

        ctrl.FixedUpdate(1, pid, 5f);

        hub.LogicToPhysic.Reader.TryRead(out var cmd).Should().BeTrue();
        var v = cmd.Should().BeOfType<SetVelocity>().Which;
        v.Velocity.X.Should().BeApproximately(5f, 0.01f);
    }
}
