using FluentAssertions;
using Vinland.Core.Infrastructure;
using Xunit;

namespace Tests.Core.Infrastructure;

public class GameThreadManagerTests : IDisposable
{
    private readonly GameThreadManager _manager = new();

    [Fact]
    public void Start_WhenCalledOnce_ShouldSucceed()
    {
        var act = () => _manager.Start();
        act.Should().NotThrow();
    }

    [Fact]
    public void Start_WhenCalledTwice_ShouldThrow()
    {
        _manager.Start();

        var act = () => _manager.Start();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Already started*");
    }

    [Fact]
    public void Stop_ShouldSignalCancellation()
    {
        _manager.Start();

        var act = () => _manager.Stop();

        act.Should().NotThrow();
    }

    [Fact]
    public void Dispose_ShouldJoinThreadsWithinTimeout()
    {
        _manager.Start();

        var act = () => _manager.Dispose();

        act.Should().NotThrow();
    }

    [Fact]
    public void Dispose_WhenNotStarted_ShouldNotThrow()
    {
        var act = () => _manager.Dispose();
        act.Should().NotThrow();
    }

    [Fact]
    public void Dispose_CalledTwice_ShouldBeIdempotent()
    {
        _manager.Start();

        _manager.Dispose();
        var act = () => _manager.Dispose();

        act.Should().NotThrow();
    }

    [Fact]
    public async Task GracefulShutdown_ShouldCompleteWithinTimeout()
    {
        _manager.Start();
        await Task.Delay(50); // дать потокам стартовать

        _manager.Stop();
        var joined = _manager.Join(TimeSpan.FromMilliseconds(500));

        joined.Should().BeTrue("threads must exit gracefully on cancellation");
    }

    [Fact]
    public void Join_WhenNotStarted_ShouldReturnTrue()
    {
        var result = _manager.Join(TimeSpan.FromMilliseconds(100));
        result.Should().BeTrue();
    }

    public void Dispose()
    {
        try { _manager.Dispose(); } catch { /* ignore */ }
    }
}