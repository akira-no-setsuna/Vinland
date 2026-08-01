using Xunit;
using FluentAssertions;
using Game.Core;
using Vector2Aether = nkast.Aether.Physics2D.Common.Vector2;
using Vector2Mono = Microsoft.Xna.Framework.Vector2;

namespace Tests.Unit
{
    public class UtilsTests
    {
        private const float PPM = 16f;
        
        [Fact]
        public void ToMono_ConvertsAetherVectorToMono()
        {
            var aether = new Vector2Aether(3.5f, -2.1f);
            var mono = aether.ToMono();
            mono.X.Should().Be(3.5f * PPM);
            mono.Y.Should().Be(-2.1f * PPM);
        }

        [Fact]
        public void ToAether_ConvertsMonoVectorToAether()
        {
            var mono = new Vector2Mono(1.2f, 4.8f);
            var aether = mono.ToAether();
            aether.X.Should().Be(1.2f / PPM);
            aether.Y.Should().Be(4.8f / PPM);
        }
    }
}