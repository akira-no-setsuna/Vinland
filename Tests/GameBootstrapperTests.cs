using Microsoft.Extensions.DependencyInjection;
using Game.Core.Infrastructure;
using Game.Core.Infrastructure.Channels;
using Game.Core.Infrastructure.Services;
using Game.Core.Infrastructure.Services.Threads;
using Game.Core.Logic;
using Game.Core.Physics;
using Game.Core.Data;
using Game.Core.Main.Input;

namespace Game.Tests;

public class GameBootstrapperTests
{
    [Fact]
    public void ConfigureServices_ResolvesAllCoreServices()
    {
        var sp = GameBootstrapper.ConfigureServices();
        sp.GetRequiredService<ChannelHub>().Should().NotBeNull();
        sp.GetRequiredService<GameThreadManager>().Should().NotBeNull();
        sp.GetRequiredService<LogicManager>().Should().NotBeNull();
        sp.GetRequiredService<PhysicsManager>().Should().NotBeNull();
        sp.GetRequiredService<DataManager>().Should().NotBeNull();
        sp.GetRequiredService<GameClock>().Should().NotBeNull();
        sp.GetRequiredService<InputSource>().Should().NotBeNull();
    }
}
