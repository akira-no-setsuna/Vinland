using System.Collections.Generic;
using Game.Core.Infrastructure.Channels;
using Serilog;

namespace Game.Core.Main.Input;

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