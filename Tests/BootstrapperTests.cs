using Microsoft.Extensions.DependencyInjection;
using Vinland.Core.Infrastructure;
using Vinland.Core.Infrastructure.Channels;
using Vinland.Core.Infrastructure.Services;
using Vinland.Core.Infrastructure.Services.Threads;
using Vinland.Core.Logic;
using Vinland.Core.Physics;
using Vinland.Core.Data;
using Vinland.Core.Main.Input;

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
