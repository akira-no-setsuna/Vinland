using System;
using Game.Core.Infrastructure;
using Game.Core.Infrastructure.Channels;
using Game.Core.Infrastructure.Channels.Commands;
using Game.Core.Logic.Entities.Data;
using Serilog;

namespace Game.Core.Logic.Entities;

public class EntityFactory(ChannelHub channelHub)
{
    public LogicEntity CreateEntity(Vector2 position, EntityData data, EntityKind kind)
    {
        var id = Guid.NewGuid();
        var commandMain = new TextureSpawn(id, position, data);
        var commandPhysic = new BodySpawn(id, position, data);

        channelHub.LogicToPhysic.Writer.TryWrite(commandPhysic);
        channelHub.LogicToMain.Writer.TryWrite(commandMain);

        Log.Information("SpawnEntityBody: ID = {id}, Pos = {pos}, type = {name},  kind = {kind}",
            id, position, data.Name, kind);

        return new LogicEntity(data)
        {
            Id = id,
            Kind = kind,
            Position = position,

            State = EntityState.Follow
        };
    }
}