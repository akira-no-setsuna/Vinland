using System;
using System.Collections.Generic;
using System.Linq;
using Game.Core.Infrastructure;
using Game.Core.Infrastructure.Channels;
using Game.Core.Infrastructure.Channels.Commands;
using Game.Core.Logic.Entities;
using Game.Core.Logic.Entities.Data;
using Serilog;

namespace Game.Core.Logic;

public class LogicManager(ChannelHub channelHub)
{
    private readonly Dictionary<Guid, LogicEntity> _entities = new();

    // Entities
    private readonly EntityFactory _factory = new(channelHub);
    private readonly PlayerController _playerController = new(channelHub);

    // Player
    private Guid _playerID;

    public void Initialize()
    {
    }

    public void LoadContent()
    {
        var entity = _factory.CreateEntity(Vector2.Zero, new HumanData(), EntityKind.Player);
        _entities.Add(entity.Id, entity);
        _playerID = FoundPlayer(_entities);
    }

    public void FixedUpdate()
    {
        PhysicReader();
        _playerController.FixedUpdate(_playerID, _entities[_playerID].Speed);
    }

    private void PhysicReader()
    {
        while (channelHub.PhysicsToMain.Reader.TryRead(out var physicsCommand))
            switch (physicsCommand)
            {
                case PositionUpdate entityPosition:
                    UpdatePositions(entityPosition);
                    break;
                default:
                    Log.Warning("Physics command {cmd} not complied", physicsCommand);
                    break;
            }
    }

    // Update logic entities positions
    private void UpdatePositions(PositionUpdate entityPosition)
    {
        if (!_entities.TryGetValue(entityPosition.EntityID, out var logicEntity))
        {
            Log.Warning("EntityID: {id} logic not found", entityPosition.EntityID);
            return;
        }

        logicEntity.Position = entityPosition.Position;
    }

    private Guid FoundPlayer(Dictionary<Guid, LogicEntity> entities)
    {
        var player = entities.Values.FirstOrDefault(e => e.Kind == EntityKind.Player);
        if (player != null)
        {
            channelHub.LogicToMain.Writer.TryWrite(new SetPlayer(player.Id));
            return player.Id;
        }

        Log.Error("Player not found");
        return Guid.Empty;
    }
}