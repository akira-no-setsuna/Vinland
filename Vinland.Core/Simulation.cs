using Serilog;
using Vinland.Core.Entities;
using Vinland.Core.Infrastructure.Channels;
using Vinland.Core.Infrastructure.Services.Threads;
using EntityKind = Vinland.Core.Entities.EntityKind;
using SpawnCommand = Vinland.Core.Infrastructure.Channels.SpawnCommand;

namespace Vinland.Core;

public class Simulation(ChannelHub channelHub, EntityManager entityManager) : BaseThread
{
    // Entities
    private readonly PlayerController _playerController = new(channelHub);
    private readonly Queue<SpawnCommand> _spawnCommands = new();

    // Player
    private long _playerID;
    private float _speed = 1.4f;


    protected override void Prepare()
    {
        _playerID = entityManager.Spawn(new SpawnCommand(null ,EntityKind.Player, Vector2.Zero));
        entityManager.Spawn(new SpawnCommand(null ,EntityKind.Enemy, Vector2.Zero));
    }

    protected override void Update(long tick, float deltaTime)
    {
        HostReader();
        EntitiesSpawn();
        _playerController.Update(_playerID, _speed);
    }

    private void HostReader()
    {
        while (channelHub.Host.Reader.TryRead(out var mainCommand))
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
            entityManager.Spawn(command);
        }
    }

    // Update logic entities positions
    private void UpdatePositions(PositionSnapshot snapshot)
    {
        if (snapshot.Entities.Count != snapshot.Positions.Count)
        {
            Log.Error("Snapshot mismatch: {Ids} ids, {Pos} positions",
                snapshot.Entities.Count, snapshot.Positions.Count);
            return;
        }
        
        for (int i = 0; i < snapshot.Entities.Count; i++)
        {
            entityManager.UpdatePositions(snapshot.Entities[i], snapshot.Positions[i]);
        }
    }
}