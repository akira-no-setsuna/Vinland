using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.ObjectPool;
using Serilog;
using Vinland.Core.Infrastructure;
using Xunit;

namespace Tests.Core.Infrastructure;

public class DependencyInjectionTests : IDisposable
{
    private readonly IServiceProvider _provider;

    public DependencyInjectionTests()
    {
        _provider = DependencyInjection.ConfigureServices();
    }

    [Fact]
    public void Resolve_ILogger_ShouldReturnSerilogInstance()
    {
        var logger = _provider.GetRequiredService<ILogger>();

        logger.Should().NotBeNull();
        logger.Should().BeOfType<Serilog.Core.Logger>();
    }

    [Fact]
    public void Resolve_ChannelHub_ShouldReturnSingleton()
    {
        var a = _provider.GetRequiredService<ChannelHub>();
        var b = _provider.GetRequiredService<ChannelHub>();

        a.Should().BeSameAs(b, "ChannelHub must be singleton");
    }

    [Fact]
    public void Resolve_ObjectPoolProvider_ShouldBeDefault()
    {
        var provider = _provider.GetRequiredService<ObjectPoolProvider>();

        provider.Should().BeOfType<DefaultObjectPoolProvider>();
    }

    [Fact]
    public void Resolve_GameThreadManager_ShouldReturnSingleton()
    {
        var a = _provider.GetRequiredService<GameThreadManager>();
        var b = _provider.GetRequiredService<GameThreadManager>();

        a.Should().BeSameAs(b);
    }

    [Fact]
    public void Resolve_UnknownService_ShouldThrow()
    {
        var act = () => _provider.GetRequiredService<string>();

        act.Should().Throw<InvalidOperationException>();
    }

    public void Dispose()
    {
        // Изолируем глобальный Serilog state между тестами
        Log.Logger = null;
    }
}