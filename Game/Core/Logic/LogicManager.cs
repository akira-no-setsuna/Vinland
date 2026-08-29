using System;
using System.Collections.Generic;
using System.Linq;
using Game.Core.Data.ConfigClasses;
using Game.Core.Infrastructure;
using Game.Core.Infrastructure.Channels;
using Game.Core.Infrastructure.Channels.Commands;
using Game.Core.Infrastructure.Services.Threads;
using Game.Core.Logic.Entities;
using Serilog;

namespace Game.Core.Logic;

public class LogicManager(ChannelHub channelHub) : BaseThread
{
    private readonly Dictionary<Guid, LogicEntity> _entities = new();
    private readonly Dictionary<string, EntityConfig> _entitiesConfigs = new();
    private readonly Queue<SpawnCommand> _spawnCommands = new();

    // Entities
    private readonly EntityFactory _factory = new(channelHub);
    private readonly PlayerController _playerController = new(channelHub);

    // Player
    private Guid _playerID;
    
    private bool _dataLoaded;

    protected override void FixedUpdate(long tick, float deltaTime)
    {
        DataReader();
        if (!_dataLoaded) return;

        EntitiesSpawn();
        PhysicReader();

        if (!FoundPlayer()) return;
        _playerController.FixedUpdate(tick, _playerID, _entities[_playerID].Speed);
    }

    private void PlayerSpawn()
    {
        _spawnCommands.Enqueue(new SpawnCommand(0, Vector2.Zero, _entitiesConfigs["human"], EntityKind.Player));
    }
    
    private void EntitiesSpawn()
    {

        while (_spawnCommands.TryDequeue(out var command))
        {
            var entity = _factory.CreateEntity(CurrentTick, command);
            _entities.Add(entity.Id, entity);
        }
    }

    private void PhysicReader()
    {
        while (channelHub.PhysicsToLogic.Reader.TryRead(out var physicsCommand))
            switch (physicsCommand)
            {
                case PositionsUpdate positionBuffer:
                    UpdatePositions(positionBuffer);
                    break;
                default:
                    Log.Warning("Physics command {cmd} not complied", physicsCommand);
                    break;
            }
    }
    
    private void DataReader()
    {
        while (channelHub.DataToLogic.Reader.TryRead(out var dataCommand))
            switch (dataCommand)
            {
                case EntityConfigs entityConfigs:
                    foreach (var entityConfig in entityConfigs.Configs)
                        _entitiesConfigs.Add(entityConfig.Key, entityConfig.Value);
                    break;
                
                case DataLoaded dataLoaded:
                    if(!dataLoaded.Success) break;
                    _dataLoaded =  true;
                    PlayerSpawn();
                    break;
                
                default:
                    Log.Warning("Data command {cmd} not complied", dataCommand);
                    break;
            }
    }

    // Update logic entities positions
    private void UpdatePositions(PositionsUpdate positionBuffer)
    {
        foreach (var position in positionBuffer.Positions)
        {
            if (_entities.TryGetValue(position.EntityID, out var logicEntity))
                logicEntity.Position = position.Position;
            else Log.Warning("EntityID: {id} logic not found", position.EntityID);
        }
    }
    
    private bool FoundPlayer()
    {
        if (_playerID != Guid.Empty)
            return true;
        
        var player = _entities.Values.FirstOrDefault(e => e.Kind == EntityKind.Player);
        if (player != null)
        {
            channelHub.LogicToMain.Writer.TryWrite(new SetPlayer(CurrentTick, player.Id));
            _playerID = player.Id;
            return true;
        }
        
        Log.Error("Player not found");
        return false;
    }
}