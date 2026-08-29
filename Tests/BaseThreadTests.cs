using Game.Core.Infrastructure.Services.Threads;

namespace Game.Tests;

public class BaseThreadTests
{
    private class Stub : BaseThread
    {
        public int Calls { get; private set; }
        protected override void FixedUpdate(long tick, float dt) => Calls++;
    }

    [Fact]
    public void ManualUpdate_WithoutStart_SkipsFixedUpdate()
    {
        var t = new Stub();
        t.ManualUpdate(1, 0.016f);
        t.Calls.Should().Be(0);
    }

    [Fact]
    public void ManualUpdate_AfterStart_CallsFixedUpdate()
    {
        var t = new Stub();
        t.Start();
        t.ManualUpdate(5, 0.016f);
        t.Calls.Should().Be(1);
    }
}
