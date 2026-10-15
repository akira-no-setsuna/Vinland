using System;
using Vinland.Core.Infrastructure.Services.Threads;

namespace Game.Tests;

public class ThreadManagerTests
{
    [Fact]
    public void Start_ThrowsOnDoubleStart()
    {
        using var mgr = new ThreadManager();
        mgr.Start();
        var act = () => mgr.Start();
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void StopAndJoin_CompletesGracefully()
    {
        using var mgr = new ThreadManager();
        mgr.Start();
        mgr.Stop();
        mgr.Join(TimeSpan.FromSeconds(2)).Should().BeTrue();
    }
}
