using Microsoft.Extensions.DependencyInjection;
using Vinland.Monogame.Core.Infrastructure;
using Vinland.Monogame.Core.Infrastructure.Channels;
using Vinland.Monogame.Core.Infrastructure.Services;
using Vinland.Monogame.Core.Infrastructure.Services.Threads;
using Vinland.Monogame.Core.Logic;
using Vinland.Monogame.Core.Physics;
using Vinland.Monogame.Core.Data;
using Vinland.Monogame.Core.Main.Input;

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
