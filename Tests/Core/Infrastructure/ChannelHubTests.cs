using FluentAssertions;
using nkast.Aether.Physics2D.Common;
using Vinland.Core.Infrastructure;
using Vinland.Core.Input;
using Xunit;

namespace Tests.Core.Infrastructure;

public class ChannelHubTests
{
    private readonly ChannelHub _hub = new();

    [Fact]
    public async Task PhysicsToMain_WriteAndRead_ShouldDeliverMessage()
    {
        // Arrange
        var update = new PhysicsUpdate("Player", new Vector2(10f, 20f));

        // Act
        // WriteAsync возвращает ValueTask (не ValueTask<bool>), поэтому просто await
        await _hub.PhysicsToMain.Writer.WriteAsync(update);
        var readOk = _hub.PhysicsToMain.Reader.TryRead(out var received);

        // Assert
        readOk.Should().BeTrue();
        received.Should().NotBeNull();
        received!.EntityId.Should().Be("Player");
        received.Position.X.Should().Be(10f);
        received.Position.Y.Should().Be(20f);
    }

    [Fact]
    public void BoundedChannel_WhenFull_ShouldDropOldest()
    {
        // Arrange — capacity = 1
        var first = new PhysicsUpdate("A", Vector2.Zero);
        var second = new PhysicsUpdate("B", Vector2.Zero);

        // Act
        _hub.PhysicsToMain.Writer.TryWrite(first);
        _hub.PhysicsToMain.Writer.TryWrite(second); // drops first
        var readOk = _hub.PhysicsToMain.Reader.TryRead(out var result);

        // Assert
        readOk.Should().BeTrue();
        result.Should().NotBeNull();
        result!.EntityId.Should().Be("B", "DropOldest policy keeps the newest");
    }

    [Fact]
    public async Task ConcurrentWriters_ShouldNotCorruptData()
    {
        // Arrange
        const int writerCount = 8;
        const int messagesPerWriter = 500;
        var received = new System.Collections.Concurrent.ConcurrentBag<PhysicsUpdate>();
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

        // Act — параллельная запись из разных потоков
        var writers = Enumerable.Range(0, writerCount).Select(i =>
            Task.Run(async () =>
            {
                for (int j = 0; j < messagesPerWriter && !cts.Token.IsCancellationRequested; j++)
                {
                    // TryWrite — синхронный, без аллокаций, без await
                    _hub.PhysicsToMain.Writer.TryWrite(
                        new PhysicsUpdate($"E{i}_{j}", new Vector2(i, j)));
                }
            }, cts.Token)).ToArray();

        // Читатель в отдельном потоке — использует WaitToReadAsync с CancellationToken
        var reader = Task.Run(async () =>
        {
            try
            {
                while (!cts.Token.IsCancellationRequested)
                {
                    // Ждём появления данных или отмены
                    if (await _hub.PhysicsToMain.Reader.WaitToReadAsync(cts.Token))
                    {
                        while (_hub.PhysicsToMain.Reader.TryRead(out var u))
                        {
                            received.Add(u);
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // drain remaining
                while (_hub.PhysicsToMain.Reader.TryRead(out var u))
                {
                    received.Add(u);
                }
            }
        }, cts.Token);

        await Task.WhenAll(writers);
        await Task.Delay(200, cts.Token); // drain
        cts.Cancel();

        try { await reader; }
        catch (OperationCanceledException) { /* expected */ }

        // Assert — ни одно сообщение не должно быть повреждено
        received.Should().OnlyContain(u => u.EntityId.StartsWith("E"));
        received.Count.Should().BeGreaterThan(0, "should receive at least some messages");
    }

    [Fact]
    public void AllChannels_ShouldBeIndependent()
    {
        // Act — запись в один канал не должна влиять на другие
        _hub.MainToLogic.Writer.TryWrite(new InputCommand(true, false, false, false, false));
        _hub.LogicToMain.Writer.TryWrite(new Vinland.Core.Logic.LogicCommand());

        // Assert
        _hub.PhysicsToMain.Reader.TryRead(out _).Should().BeFalse();
        _hub.MainToLogic.Reader.TryRead(out _).Should().BeTrue();
        _hub.LogicToMain.Reader.TryRead(out _).Should().BeTrue();
    }

    [Fact]
    public void EmptyChannel_TryRead_ShouldReturnFalse()
    {
        var readOk = _hub.PhysicsToMain.Reader.TryRead(out var result);

        readOk.Should().BeFalse();
        result.Should().BeNull("empty channel returns default value");
    }
}