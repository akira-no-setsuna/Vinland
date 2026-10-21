using System;
using Vinland.Monogame.Data.Config;
using Vinland.Monogame.Infrastructure.Channels;
using Vinland.Monogame.Infrastructure.Channels.Commands;
using Vinland.Monogame.Logic.Entities;

namespace Game.Tests;

public class EntityFactoryTests
{
    [Fact]
    public void CreateEntity_SendsCommandsToChannels()
    {
        var hub = new ChannelHub();
        var factory = new EntityFactory(hub);
        var cfg = new EntityConfig { Species = "h", MaxHealth = 100, Speed = 5, Radius = 1, Density = 1, TextureKey = "k" };
        var cmd = new SpawnCommand(1, Vector2.Zero, cfg, EntityKind.Player);

        var e = factory.CreateEntity(1, cmd);

        hub.LogicToMain.Reader.TryRead(out var mc).Should().BeTrue();
        mc.Should().BeOfType<TextureSpawn>().Which.EntityID.Should().Be(e.Id);

        hub.LogicToPhysics.Reader.TryRead(out var pc).Should().BeTrue();
        pc.Should().BeOfType<BodySpawn>().Which.EntityID.Should().Be(e.Id);
    }
}
