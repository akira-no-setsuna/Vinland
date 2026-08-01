using Xunit;
using FluentAssertions;
using Game.Core.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.ObjectPool;
using Serilog;

namespace Tests.Unit
{
    public class DependencyInjectionTests
    {
        [Fact]
        public void ConfigureServices_RegistersExpectedServices()
        {
            var provider = DependencyInjection.ConfigureServices();

            provider.GetService<ChannelHub>().Should().NotBeNull();
            provider.GetService<ILogger>().Should().NotBeNull();
            provider.GetService<GameThreadManager>().Should().NotBeNull();
            provider.GetService<ObjectPoolProvider>().Should().NotBeNull();
        }

        [Fact]
        public void ConfigureServices_LoggerIsSerilogLogger()
        {
            var provider = DependencyInjection.ConfigureServices();
            var logger = provider.GetService<ILogger>();
            logger.Should().BeAssignableTo<Serilog.ILogger>();
        }
    }
}