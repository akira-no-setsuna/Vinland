using System.Threading.Tasks;
using Game.Core.Infrastructure.Channels;
using Game.Core.Infrastructure.Channels.Commands;

namespace Game.Tests;

public class ChannelHubTests
{
    [Fact]
    public void WriteAndRead_SingleMessage_Works()
    {
        var hub = new ChannelHub();
        var id = System.Guid.NewGuid();
        hub.LogicToMain.Writer.TryWrite(new SetPlayer(1, id));
        hub.LogicToMain.Reader.TryRead(out var cmd).Should().BeTrue();
        cmd.Should().BeOfType<SetPlayer>().Which.EntityID.Should().Be(id);
    }

    [Fact]
    public async Task ConcurrentWrites_DoNotThrow()
    {
        var hub = new ChannelHub();
        var tasks = new Task[500];
        for (int i = 0; i < tasks.Length; i++)
            tasks[i] = Task.Run(() => hub.LogicToMain.Writer.TryWrite(new SetPlayer(i, System.Guid.NewGuid())));
        await Task.WhenAll(tasks);
        int read = 0;
        while (hub.LogicToMain.Reader.TryRead(out _)) read++;
        read.Should().BeGreaterThan(0);
    }
}
