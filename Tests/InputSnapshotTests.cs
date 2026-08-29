using Game.Core.Main.Input;

namespace Game.Tests;

public class InputSnapshotTests
{
    [Fact]
    public void MoveDirection_NoInput_ReturnsZero()
    {
        var s = new InputSnapshot(false, false, false, false);
        s.MoveDirection.Should().Be(Vector2.Zero);
    }

    [Fact]
    public void MoveDirection_Diagonal_IsNormalized()
    {
        var s = new InputSnapshot(false, true, false, true);
        var d = s.MoveDirection;
        d.Length().Should().BeApproximately(1f, 0.001f);
    }
}
