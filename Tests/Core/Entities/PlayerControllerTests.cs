using Xunit;
using FluentAssertions;
using Game.Core.Entities;
using Vinland.Core.Input;
using nkast.Aether.Physics2D.Dynamics;
using Vector2Aether = nkast.Aether.Physics2D.Common.Vector2;

namespace Tests.Integration
{
    public class PlayerControllerTests
    {
        [Fact]
        public void FixedUpdate_WithMoveUp_SetsVelocityUp()
        {
            var world = new World(Vector2Aether.Zero);
            var body = world.CreateBody();
            body.BodyType = BodyType.Dynamic;
            var controller = new PlayerController(body);
            var input = new InputCommand(false, false, true, false, false);
            controller.FixedUpdate(input);
            body.LinearVelocity.Y.Should().Be(-50f);
            body.LinearVelocity.X.Should().Be(0);
        }

        [Fact]
        public void FixedUpdate_WithMoveDown_SetsVelocityDown()
        {
            var world = new World(Vector2Aether.Zero);
            var body = world.CreateBody();
            body.BodyType = BodyType.Dynamic;
            var controller = new PlayerController(body);
            var input = new InputCommand(false, false, false, true, false);
            controller.FixedUpdate(input);
            body.LinearVelocity.Y.Should().Be(50f);
        }

        [Fact]
        public void FixedUpdate_WithMoveLeft_SetsVelocityLeft()
        {
            var world = new World(Vector2Aether.Zero);
            var body = world.CreateBody();
            body.BodyType = BodyType.Dynamic;
            var controller = new PlayerController(body);
            var input = new InputCommand(true, false, false, false, false);
            controller.FixedUpdate(input);
            body.LinearVelocity.X.Should().Be(-50f);
        }

        [Fact]
        public void FixedUpdate_WithMoveRight_SetsVelocityRight()
        {
            var world = new World(Vector2Aether.Zero);
            var body = world.CreateBody();
            body.BodyType = BodyType.Dynamic;
            var controller = new PlayerController(body);
            var input = new InputCommand(false, true, false, false, false);
            controller.FixedUpdate(input);
            body.LinearVelocity.X.Should().Be(50f);
        }

        [Fact]
        public void FixedUpdate_WithCombinedDirection_NormalizesVelocity()
        {
            var world = new World(Vector2Aether.Zero);
            var body = world.CreateBody();
            body.BodyType = BodyType.Dynamic;
            var controller = new PlayerController(body);
            var input = new InputCommand(true, false, true, false, false);
            controller.FixedUpdate(input);
            var expected = new Vector2Aether(-1, -1);
            expected.Normalize();
            expected *= 50f;
            body.LinearVelocity.X.Should().BeApproximately(expected.X, 1e-6f);
            body.LinearVelocity.Y.Should().BeApproximately(expected.Y, 1e-6f);
        }

        [Fact]
        public void FixedUpdate_WithNoInput_SetsVelocityZero()
        {
            var world = new World(Vector2Aether.Zero);
            var body = world.CreateBody();
            body.BodyType = BodyType.Dynamic;
            var controller = new PlayerController(body);
            var input = new InputCommand(false, false, false, false, false);
            controller.FixedUpdate(input);
            body.LinearVelocity.Should().Be(Vector2Aether.Zero);
        }
    }
}