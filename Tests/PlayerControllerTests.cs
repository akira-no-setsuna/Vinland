using System;
using Vinland.Monogame.Core.Infrastructure.Channels;
using Vinland.Monogame.Core.Infrastructure.Channels.Commands;
using Vinland.Monogame.Core.Main.Input;
using Vinland.Monogame.Core.Logic.Entities;
using Vinland.Monogame.Main.Input;

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

        hub.LogicToPhysics.Reader.TryRead(out var cmd).Should().BeTrue();
        var v = cmd.Should().BeOfType<SetVelocity>().Which;
        v.Velocity.X.Should().BeApproximately(5f, 0.01f);
    }
}
