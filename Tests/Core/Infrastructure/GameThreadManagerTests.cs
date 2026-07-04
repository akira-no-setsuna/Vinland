using Xunit;
using FluentAssertions;
using System;
using System.Threading;
using Game.Core.Infrastructure;

namespace Tests.Unit
{
    public class GameThreadManagerTests : IDisposable
    {
        private readonly GameThreadManager _manager;

        public GameThreadManagerTests()
        {
            _manager = new GameThreadManager();
        }

        [Fact]
        public void Start_DoesNotThrow()
        {
            Action act = () => _manager.Start();
            act.Should().NotThrow();
        }

        [Fact]
        public void Start_WhenAlreadyStarted_ThrowsInvalidOperationException()
        {
            _manager.Start();
            Action act = () => _manager.Start();
            act.Should().Throw<InvalidOperationException>().WithMessage("Already started");
        }

        [Fact]
        public void Stop_DoesNotThrow()
        {
            _manager.Start();
            Action act = () => _manager.Stop();
            act.Should().NotThrow();
        }

        [Fact]
        public void Dispose_StopsThreadsAndDisposes()
        {
            _manager.Start();
            Action act = () => _manager.Dispose();
            act.Should().NotThrow();
        }

        [Fact]
        public void Join_ReturnsTrueWhenThreadsComplete()
        {
            _manager.Start();
            _manager.Stop();
            var result = _manager.Join(TimeSpan.FromMilliseconds(500));
            result.Should().BeTrue();
        }

        [Fact]
        public void Dispose_WhenNotStarted_DoesNotThrow()
        {
            Action act = () => _manager.Dispose();
            act.Should().NotThrow();
        }

        public void Dispose()
        {
            _manager?.Dispose();
        }
    }
}