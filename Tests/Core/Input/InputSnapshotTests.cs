using Xunit;
using FluentAssertions;
using Game.Core.Main.Input;
using Vector2Aether = nkast.Aether.Physics2D.Common.Vector2;

namespace Tests.Unit
{
    public class InputSnapshotTests
    {
        [Fact]
        public void MoveDirection_WithUp_ReturnsUp()
        {
            var cmd = new InputSnapshot(false, false, true, false, false);
            cmd.MoveDirection.Should().Be(new Vector2Aether(0, -1));
        }

        [Fact]
        public void MoveDirection_WithDown_ReturnsDown()
        {
            var cmd = new InputSnapshot(false, false, false, true, false);
            cmd.MoveDirection.Should().Be(new Vector2Aether(0, 1));
        }

        [Fact]
        public void MoveDirection_WithLeft_ReturnsLeft()
        {
            var cmd = new InputSnapshot(true, false, false, false, false);
            cmd.MoveDirection.Should().Be(new Vector2Aether(-1, 0));
        }

        [Fact]
        public void MoveDirection_WithRight_ReturnsRight()
        {
            var cmd = new InputSnapshot(false, true, false, false, false);
            cmd.MoveDirection.Should().Be(new Vector2Aether(1, 0));
        }

        [Fact]
        public void MoveDirection_WithUpAndLeft_ReturnsDiagonalVector()
        {
            var cmd = new InputSnapshot(true, false, true, false, false);
            cmd.MoveDirection.Should().Be(new Vector2Aether(-1, -1));
        }

        [Fact]
        public void MoveDirection_WithOppositeDirections_ReturnsZero()
        {
            var cmd = new InputSnapshot(true, true, false, false, false);
            cmd.MoveDirection.Should().Be(Vector2Aether.Zero);
        }
    }
}