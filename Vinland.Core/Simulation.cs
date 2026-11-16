using Serilog;
using Vinland.Core.Entities;
using Vinland.Core.Infrastructure.Channels;
using Vinland.Core.Infrastructure.Services.Threads;
using EntityKind = Vinland.Core.Entities.EntityKind;
using SpawnCommand = Vinland.Core.Infrastructure.Channels.SpawnCommand;

namespace Vinland.Core;

public class Simulation(ChannelHub channelHub, EntityManager entityManager) : BaseThread
{
    public override string ThreadName { get; } = "Simulation";
    
    // Entities
    private PlayerController _playerController;

    // Player
    private long _playerID;
    private readonly float _speed = 1.4f;

    private readonly Queue<SpawnCommand> _spawnCommands = new();


    protected override void Prepare()
    {
        _playerID = entityManager.Spawn(new SpawnCommand(null, EntityKind.Player, Vector2.Zero));
        entityManager.Spawn(new SpawnCommand(null, EntityKind.Enemy, Vector2.One));
        _playerController = new PlayerController(channelHub, _playerID);
    }

    protected override void Update(float deltaTime)
    {
        HostReader();
        EntitiesSpawn();

        _playerController.Update(_speed);
        entityManager.Update();
    }

    private void HostReader()
    {
        while (channelHub.Host.Reader.TryRead(out var mainCommand))
            switch (mainCommand)
            {
                case PositionSnapshot snapshot:
                    entityManager.UpdatePositions(snapshot);
                    break;
                case RayCastResponse response:
                    entityManager.SightUpdate(response);
                    break;
                default:
                    Log.Warning("Main command {cmd} not complied", mainCommand);
                    break;
            }
    }

    private void EntitiesSpawn()
    {
        while (_spawnCommands.TryDequeue(out var command)) entityManager.Spawn(command);
    }
}