using Serilog;
using Vinland.Core.Entities;
using Vinland.Core.Infrastructure.Channels;
using Vinland.Core.Infrastructure.Services.Threads;
using EntityKind = Vinland.Core.Entities.EntityKind;
using SpawnCommand = Vinland.Core.Infrastructure.Channels.SpawnCommand;

namespace Vinland.Core;

public class Simulation(ChannelHub channelHub) : BaseThread
{
    private readonly Dictionary<Guid, Entity> _entities = new();

    // Entities
    private readonly PlayerController _playerController = new(channelHub);
    private readonly Queue<SpawnCommand> _spawnCommands = new();

    // Player
    private Guid _playerID;
    private float _speed = 1.4f;


    protected override void Prepare()
    {
        _spawnCommands.Enqueue(new SpawnCommand(Guid.NewGuid(), EntityKind.Player, Vector2.Zero));
    }

    protected override void FixedUpdate(long tick, float deltaTime)
    {
        RuntimeReader();
        EntitiesSpawn();
    }

    private void RuntimeReader()
    {
        while (channelHub.Runtime.Reader.TryRead(out var mainCommand))
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
            Log.Information("Spawning entity {entity}", command.EntityID);
            
            var entity = new Entity()
            {
                Id = command.EntityID,
                Kind = command.Kind,
                Position = command.Position,
            };
            
            _playerID = command.EntityID;
            
            _entities.Add(entity.Id, entity);
            channelHub.Gameplay.Writer.TryWrite(command);
        }
    }

    // Update logic entities positions
    private void UpdatePositions(PositionSnapshot snapshot)
    {
        for (int i = 0; i < snapshot.EntityID.Length; i++)
        {
            if (_entities.TryGetValue(snapshot.EntityID[i], out var logicEntity))
                logicEntity.Position = snapshot.Position[i];
        }
    }
}