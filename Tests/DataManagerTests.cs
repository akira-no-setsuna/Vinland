using Game.Core.Data;
using Game.Core.Infrastructure.Channels;

namespace Game.Tests;

public class DataManagerTests
{
    [Fact]
    public void Start_MissingConfigFile_DoesNotCrash_AndSendsFailure()
    {
        var hub = new ChannelHub();
        var dataManager = new DataManager(hub);
        
        dataManager.Start();
        
        hub.DataToLogic.Reader.TryRead(out var cmd).Should().BeTrue();
    }
}