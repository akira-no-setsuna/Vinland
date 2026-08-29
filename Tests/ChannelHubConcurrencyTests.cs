using Game.Core.Infrastructure.Channels;
using Game.Core.Infrastructure.Channels.Commands;

namespace Game.Tests;

public class ChannelHubConcurrencyTests
{
    [Fact]
    public async Task ConcurrentReadWrite_FromMultipleThreads_DoesNotDeadlock()
    {
        var hub = new ChannelHub();
        var cts = new System.Threading.CancellationTokenSource();
        int readCount = 0;

        // Поток 1: Постоянное чтение
        var readerTask = Task.Run(async () =>
        {
            while (!cts.IsCancellationRequested)
            {
                if (hub.LogicToMain.Reader.TryRead(out _))
                {
                    readCount++;
                }
                await Task.Delay(1);
            }
        });

        // Поток 2 и 3: Активная запись
        var writerTask1 = Task.Run(() =>
        {
            for (int i = 0; i < 1000; i++)
                hub.LogicToMain.Writer.TryWrite(new SetPlayer(i, Guid.NewGuid()));
        });
        
        var writerTask2 = Task.Run(() =>
        {
            for (int i = 0; i < 1000; i++)
                hub.LogicToMain.Writer.TryWrite(new SetPlayer(i, Guid.NewGuid()));
        });

        await Task.WhenAll(writerTask1, writerTask2);
        cts.Cancel();
        await readerTask;

        readCount.Should().BeGreaterThan(0);
        readCount.Should().BeLessThanOrEqualTo(2000);
    }
}