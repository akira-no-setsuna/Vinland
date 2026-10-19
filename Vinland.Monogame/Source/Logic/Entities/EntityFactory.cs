using System;
using Vinland.Monogame.Data.Config;
using Vinland.Monogame.Infrastructure.Channels;
using Vinland.Monogame.Infrastructure.Channels.Commands;

namespace Vinland.Monogame.Logic.Entities;

public class EntityFactory(ChannelHub channelHub)
{
    public LogicEntity CreateEntity(long tick, SpawnCommand spawnCommand)
    {
        var id = Guid.NewGuid();
        var commandMain = new TextureSpawn(tick, id, spawnCommand.Position, spawnCommand.Config.TextureKey);
        var commandPhysic = new BodySpawn(tick, id, spawnCommand.Position, spawnCommand.Config.Radius,
            spawnCommand.Config.Density);

        channelHub.LogicToPhysics.Writer.TryWrite(commandPhysic);
        channelHub.LogicToMain.Writer.TryWrite(commandMain);

        return new LogicEntity(spawnCommand.Config)
        {
            Id = id,
            Kind = spawnCommand.Kind,
            Position = spawnCommand.Position,

            State = EntityState.Follow
        };
    }
}

public record SpawnCommand(
    long Tick,
    Vector2 Position,
    EntityConfig Config,
    EntityKind Kind
);