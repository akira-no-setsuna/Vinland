using FluentAssertions;
using Vinland.Core;
using Vector2Aether = nkast.Aether.Physics2D.Common.Vector2;
using Vector2Mono = Microsoft.Xna.Framework.Vector2;
using Xunit;

namespace Tests.Core;

public class UtilsTests
{
    [Fact]
    public void ToMono_ShouldPreserveComponents()
    {
        var aether = new Vector2Aether(3.14f, -2.71f);

        var mono = aether.ToMono();

        mono.X.Should().Be(3.14f);
        mono.Y.Should().Be(-2.71f);
    }

    [Fact]
    public void ToAether_ShouldPreserveComponents()
    {
        var mono = new Vector2Mono(1.5f, 9.9f);

        var aether = mono.ToAether();

        aether.X.Should().Be(1.5f);
        aether.Y.Should().Be(9.9f);
    }

    [Fact]
    public void RoundTrip_ShouldBeIdentity()
    {
        var original = new Vector2Mono(42f, -7f);

        var roundTripped = original.ToAether().ToMono();

        roundTripped.X.Should().Be(original.X);
        roundTripped.Y.Should().Be(original.Y);
    }

    [Fact]
    public void ZeroVector_ShouldConvertCleanly()
    {
        Vector2Aether.Zero.ToMono().Should().Be(Vector2Mono.Zero);
        Vector2Mono.Zero.ToAether().Should().Be(Vector2Aether.Zero);
    }
}