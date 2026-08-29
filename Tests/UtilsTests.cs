using Game.Core.Infrastructure;

namespace Game.Tests;

public class UtilsTests
{
    [Fact]
    public void ToWorld_DividesByPPM()
    {
        var r = new Vector2(32, 48).ToWorld();
        r.X.Should().Be(2f);
        r.Y.Should().Be(3f);
    }

    [Fact]
    public void ToScreen_MultipliesByPPM()
    {
        var r = new Vector2(2, 3).ToScreen();
        r.X.Should().Be(32f);
        r.Y.Should().Be(48f);
    }
}
