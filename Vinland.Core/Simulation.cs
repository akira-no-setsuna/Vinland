using Serilog;
using Vinland.Core.Entities;
using Vinland.Core.Infrastructure.Channels;
using Vinland.Core.Infrastructure.Services.Threads;
using EntityKind = Vinland.Core.Entities.EntityKind;
using SpawnCommand = Vinland.Core.Infrastructure.Channels.SpawnCommand;

namespace Vinland.Core;

public class Simulation(ChannelHub hub) : BaseThread
{
    private readonly Dictionary<Guid, Entity> _entities = new();

    // Entities
    private readonly PlayerController _playerController = new(hub);
    private readonly Queue<SpawnCommand> _spawnCommands = new();

    // Player
    private Guid _playerID;
    private float _speed = 1.4f;


    protected override void Prepare()
    {
        _spawnCommands.Enqueue(new SpawnCommand(new Guid(), EntityKind.Player, Vector2.Zero));
    }

    protected override void FixedUpdate(long tick, float deltaTime)
    {
        MainReader();
        EntitiesSpawn();
        
        if (!FoundPlayer()) return;
        _playerController.FixedUpdate(_playerID, _speed);
    }

    private void MainReader()
    {
        while (hub.Runtime.Reader.TryRead(out var mainCommand))
            switch (mainCommand)
            {
                case PositionSnapshot snapshot:
                    UpdatePositions(snapshot);
                    break;
                default:
                    Log.Warning("Main command {cmd} not complied", mainCommand);
                    break;
            }
    }
    
    private void EntitiesSpawn()
    {
        while (_spawnCommands.TryDequeue(out var command))
        {
            var entity = new Entity()
            {
                Id = command.EntityID,
                Kind = command.Kind,
                Position = command.Position,
            };
            
            _entities.Add(entity.Id, entity);
            hub.Gameplay.Writer.TryWrite(command);
        }
    }

    // Update logic entities positions
    private void UpdatePositions(PositionSnapshot snapshot)
    {
        for (int i = 0; i < snapshot.EntityID.Length; i++)
        {
            if (_entities.TryGetValue(snapshot.EntityID[i], out var logicEntity))
                logicEntity.Position = snapshot.Position[i];
            else Log.Warning("EntityID: {id} logic not found", snapshot.Position[i]);
        }
    }

    private bool FoundPlayer()
    {
        if (_playerID != Guid.Empty)
            return true;

        var player = _entities.Values.FirstOrDefault(e => e.Kind == EntityKind.Player);
        if (player != null)
        {
            _playerID = player.Id;
            return true;
        }

        Log.Error("Player not found");
        return false;
    }
}