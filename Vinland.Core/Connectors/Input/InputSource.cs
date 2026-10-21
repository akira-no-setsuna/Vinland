using Serilog;
using Vinland.Core.Infrastructure.Channels;

namespace Vinland.Core.Connectors.Input;

public abstract class InputSource(ChannelHub channelHub)
{
    protected Queue<InputEvent> InputEvents = new();
    protected InputSnapshot InputSnapshot = new();

    public void Update()
    {
        ReadSnapshot();
        ReadEvents();

        if (!channelHub.InputSnapshots.Writer.TryWrite(InputSnapshot))
            Log.Warning("Failed to write input snapshot.");

        while (InputEvents.TryDequeue(out var evt))
            if (!channelHub.InputEvents.Writer.TryWrite(evt)) 
                Log.Warning("Failed to write input event.");
    }
    
    public void Release()
    {
        channelHub.InputSnapshots.Writer.TryWrite(new InputSnapshot());
    }
    
    protected abstract void ReadSnapshot();
    protected abstract void ReadEvents();
}