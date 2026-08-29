using System;
using Game.Core.Infrastructure.Services.Threads;

namespace Game.Tests;

public class GameThreadManagerTests
{
    [Fact]
    public void Start_ThrowsOnDoubleStart()
    {
        using var mgr = new GameThreadManager();
        mgr.Start();
        var act = () => mgr.Start();
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void StopAndJoin_CompletesGracefully()
    {
        using var mgr = new GameThreadManager();
        mgr.Start();
        mgr.Stop();
        mgr.Join(TimeSpan.FromSeconds(2)).Should().BeTrue();
    }
}
