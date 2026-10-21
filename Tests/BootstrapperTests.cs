using Microsoft.Extensions.DependencyInjection;
using Vinland.Monogame.Data;
using Vinland.Monogame.Infrastructure;
using Vinland.Monogame.Infrastructure.Channels;
using Vinland.Monogame.Infrastructure.Services;
using Vinland.Monogame.Infrastructure.Services.Threads;
using Vinland.Monogame.Logic;
using Vinland.Monogame.Main.Input;
using Vinland.Monogame.Physics;

namespace Game.Tests;

public class BootstrapperTests
{
    [Fact]
    public void ConfigureServices_ResolvesAllCoreServices()
    {
        var sp = Bootstrapper.ConfigureServices();
        sp.GetRequiredService<ChannelHub>().Should().NotBeNull();
        sp.GetRequiredService<ThreadManager>().Should().NotBeNull();
        sp.GetRequiredService<LogicManager>().Should().NotBeNull();
        sp.GetRequiredService<PhysicsManager>().Should().NotBeNull();
        sp.GetRequiredService<DataManager>().Should().NotBeNull();
        sp.GetRequiredService<Clock>().Should().NotBeNull();
        sp.GetRequiredService<InputSource>().Should().NotBeNull();
    }
}
