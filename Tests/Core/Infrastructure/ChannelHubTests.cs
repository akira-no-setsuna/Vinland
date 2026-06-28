using Xunit;
using FluentAssertions;
using Game.Core.Infrastructure;
using Vinland.Core.Infrastructure;

namespace Tests.Unit
{
    public class ChannelHubTests
    {
        [Fact]
        public void Constructor_InitializesChannels()
        {
            var hub = new ChannelHub();
            hub.PhysicsToMain.Should().NotBeNull();
            hub.MainToLogic.Should().NotBeNull();
            hub.LogicToMain.Should().NotBeNull();
        }

        [Fact]
        public void Channels_HaveCapacityOneAndDropOldestPolicy()
        {
            var hub = new ChannelHub();
            hub.PhysicsToMain.Writer.TryWrite(new PhysicsUpdate("test", default));
            hub.PhysicsToMain.Writer.TryWrite(new PhysicsUpdate("test2", default));

            // В канале должно быть только одно сообщение (второе)
            var reader = hub.PhysicsToMain.Reader;
            var read = reader.TryRead(out var first);
            read.Should().BeTrue();
            first.EntityId.Should().Be("test2");

            // Больше ничего нет
            reader.TryRead(out _).Should().BeFalse();
        }
    }
}