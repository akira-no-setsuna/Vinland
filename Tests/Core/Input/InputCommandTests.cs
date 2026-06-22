using FluentAssertions;
using Vinland.Core.Input;
using Xunit;

namespace Tests.Core.Input;

public class InputCommandTests
{
    [Fact]
    public void Constructor_WithAllDirections_ShouldProduceZeroVector()
    {
        // Arrange & Act
        var cmd = new InputCommand(
            MoveLeft: true,
            MoveRight: true,
            MoveUp: true,
            MoveDown: true,
            Attack: true);

        // Assert — противоположные направления взаимно уничтожаются
        cmd.MoveDirection.X.Should().Be(0f, "left and right cancel out");
        cmd.MoveDirection.Y.Should().Be(0f, "up and down cancel out");
        cmd.Attack.Should().BeTrue();
    }

    // === Одиночные направления (кардинальные) ===
    [Theory]
    [InlineData(true,  false, false, false,  1f,  0f)] // Right only
    [InlineData(false, true,  false, false, -1f,  0f)] // Left only
    [InlineData(false, false, true,  false,  0f,  1f)] // Down only  (Y+ в Aether = вниз)
    [InlineData(false, false, false, true,   0f, -1f)] // Up only    (Y- в Aether = вверх)
    public void MoveDirection_SingleDirection_ShouldReturnUnitVector(
        bool right, bool left, bool down, bool up,
        float expectedX, float expectedY)
    {
        var cmd = new InputCommand(left, right, up, down, Attack: false);

        cmd.MoveDirection.X.Should().Be(expectedX);
        cmd.MoveDirection.Y.Should().Be(expectedY);
        cmd.MoveDirection.Length().Should().BeApproximately(1f, 0.0001f,
            "single direction must produce unit vector");
    }

    // === Диагональные комбинации (НЕ нормализованы — это ответственность PlayerController) ===
    [Theory]
    [InlineData(true,  false, true,  false,  1f,  1f)] // Right + Down
    [InlineData(true,  false, false, true,   1f, -1f)] // Right + Up
    [InlineData(false, true,  true,  false, -1f,  1f)] // Left + Down
    [InlineData(false, true,  false, true,  -1f, -1f)] // Left + Up
    public void MoveDirection_DiagonalCombinations_ShouldReturnRawNotNormalized(
        bool right, bool left, bool down, bool up,
        float expectedX, float expectedY)
    {
        var cmd = new InputCommand(left, right, up, down, Attack: false);

        cmd.MoveDirection.X.Should().Be(expectedX);
        cmd.MoveDirection.Y.Should().Be(expectedY);
        cmd.MoveDirection.Length().Should().BeApproximately(1.414f, 0.001f,
            "diagonal is NOT normalized here — PlayerController.FixedUpdate does it");
    }

    [Fact]
    public void MoveDirection_AllFalse_ShouldBeZeroVector()
    {
        var cmd = new InputCommand(false, false, false, false, false);

        cmd.MoveDirection.X.Should().Be(0f);
        cmd.MoveDirection.Y.Should().Be(0f);
        cmd.MoveDirection.LengthSquared().Should().Be(0f);
    }

    [Fact]
    public void MoveDirection_Diagonal_ShouldNotBeNormalized_Here()
    {
        // Явная проверка контракта: InputCommand — сырой ввод, нормализация — в контроллере
        var cmd = new InputCommand(false, true, false, true, false); // Right+Down

        cmd.MoveDirection.Length().Should().BeApproximately(1.414f, 0.001f,
            "InputCommand returns raw direction; normalization is controller's job");
    }

    [Fact]
    public void Record_Equality_ShouldWorkByValue()
    {
        var a = new InputCommand(true, false, false, true, true);
        var b = new InputCommand(true, false, false, true, true);
        var c = new InputCommand(false, false, false, true, true);

        a.Should().Be(b);
        a.Should().NotBe(c);
    }

    [Fact]
    public void Record_StructEquality_DifferentAttackFlag_ShouldNotBeEqual()
    {
        var moveOnly = new InputCommand(true, false, false, false, Attack: false);
        var moveAndAttack = new InputCommand(true, false, false, false, Attack: true);

        moveOnly.Should().NotBe(moveAndAttack,
            "Attack flag is part of record equality");
    }
}