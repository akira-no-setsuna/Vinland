using Game.Core.Infrastructure.Services;

namespace Game.Tests;

public class GameClockTests
{
    [Fact]
    public void Tick_ExactMultiple_CallsCorrectCount()
    {
        var clock = new GameClock();
        int count = 0;
        clock.Tick(0.06f, (_, _) => count++);
        count.Should().Be(3);
        clock.CurrentTick.Should().Be(3);
    }

    [Fact]
    public void Tick_ClampsLargeDelta()
    {
        var clock = new GameClock();
        int count = 0;
        // MAX_ACCUMULATOR is 0.25f. 0.25f / (1f/60f) = 15.
        clock.Tick(1f, (_, _) => count++);
        count.Should().Be(15);
    }

    [Fact]
    public void Tick_AccumulatesSmallDeltas()
    {
        var clock = new GameClock();
        int count = 0;
        clock.Tick(0.01f, (_, _) => count++);
        count.Should().Be(0);
        clock.Tick(0.01f, (_, _) => count++);
        count.Should().Be(1);
    }
}
