using Game.Core.Data.ConfigClasses;
using Game.Core.Infrastructure;

namespace Game.Tests;

public class LogicEntityTests
{
    private static EntityConfig Cfg(float hp = 100, float speed = 5) =>
        new() { MaxHealth = hp, Speed = speed, Radius = 1, Density = 1, Species = "t", TextureKey = "t" };

    [Fact]
    public void Constructor_SetsHealthToMax()
    {
        var e = new LogicEntity(Cfg(100)) { Id = System.Guid.NewGuid(), Kind = EntityKind.Player };
        e.Health.Should().Be(100);
        e.IsDead.Should().BeFalse();
        e.State.Should().Be(EntityState.Idle);
    }

    [Fact]
    public void TakeDamage_ReducesHealth()
    {
        var e = new LogicEntity(Cfg(100)) { Id = System.Guid.NewGuid(), Kind = EntityKind.Enemy };
        e.TakeDamage(30);
        e.Health.Should().Be(70);
        e.IsDead.Should().BeFalse();
    }

    [Fact]
    public void TakeDamage_KillsAtZero()
    {
        var e = new LogicEntity(Cfg(50)) { Id = System.Guid.NewGuid(), Kind = EntityKind.Enemy };
        e.TakeDamage(50);
        e.Health.Should().Be(0);
        e.IsDead.Should().BeTrue();
    }

    [Fact]
    public void TakeDamage_Overkill_SetsDead()
    {
        var e = new LogicEntity(Cfg(50)) { Id = System.Guid.NewGuid(), Kind = EntityKind.Enemy };
        e.TakeDamage(999);
        e.IsDead.Should().BeTrue();
    }
}
