using System.Collections.Generic;
using Serilog;
using Vinland.Monogame.Core.Infrastructure.Channels;
using Vinland.Monogame.Main.Input;

namespace Vinland.Monogame.Core.Main.Input;

public abstract class InputSource(ChannelHub channelHub)
{
    protected Queue<InputEvent> InputEvents = new();
    protected InputSnapshot InputSnapshot = new();

    public void Update()
    {
        UpdateDevice();
        ReadSnapshot();
        ReadEvents();
        SavePreviousState();

        if (!channelHub.InputSnapshots.Writer.TryWrite(InputSnapshot))
            Log.Warning("Failed to write input snapshot.");

        while (InputEvents.TryDequeue(out var evt))
            if (!channelHub.InputEvents.Writer.TryWrite(evt))
                Log.Warning("Failed to write input event.");
    }

    protected abstract void UpdateDevice();
    protected abstract void ReadSnapshot();
    protected abstract void ReadEvents();
    protected abstract void SavePreviousState();
}