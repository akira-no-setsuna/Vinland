using System;
using System.Collections.Generic;
using Game.Core.Infrastructure.Channels;
using Game.Core.Infrastructure.Channels.Commands;
using Game.Core.Infrastructure.Services.Threads;
using nkast.Aether.Physics2D.Collision.Shapes;
using nkast.Aether.Physics2D.Dynamics;
using Serilog;

namespace Game.Core.Physics;

public class PhysicsManager(ChannelHub channelHub) : BaseThread
{
    private readonly Dictionary<Guid, Body> _entities = new();
    private readonly MapColliderGenerator _mapColliderGenerator = new() ;
    private readonly World _physicWorld = new (Vector2.Zero);

    protected override void Prepare()
    {
    }

    protected override void FixedUpdate(float deltaTime)
    {
        LogicReader();
        MainReader();
        _physicWorld.Step(deltaTime);
        UpdatePositions();

#if DEBUG
        CollidersRender();
#endif
    }

    private void LogicReader()
    {
        while (channelHub.LogicToPhysic.Reader.TryRead(out var logicCommand))
            switch (logicCommand)
            {
                case BodySpawn command:
                    SpawnEntitiesBody(command);
                    break;
                case SetVelocity command:
                    SetEntityVelocity(command);
                    break;
                default:
                    Log.Warning("Logic command {cmd} not complied", logicCommand);
                    break;
            }
    }

    private void MainReader()
    {
        while (channelHub.MainToPhysic.Reader.TryRead(out var mainCommand))
            switch (mainCommand)
            {
                case GenerateMapColliders command:
                    _mapColliderGenerator.InitializeFromMap(_physicWorld, command.Tilemap);
                    break;
                default:
                    Log.Warning("Main command {cmd} not complied", mainCommand);
                    break;
            }
    }

    private void CollidersRender()
    {
        channelHub.PhysicsToMain.Writer.TryWrite(new BodyListRender(_physicWorld.BodyList));
    }

    private void SetEntityVelocity(SetVelocity setVelocity)
    {
        if (_entities.TryGetValue(setVelocity.EntityID, out var body)) body.LinearVelocity = setVelocity.Velocity;
        else Log.Warning("EntityID: {id} Entity body not found", setVelocity.EntityID);
    }

    private void SpawnEntitiesBody(BodySpawn spawn)
    {
        var body = _physicWorld.CreateBody();
        body.BodyType = BodyType.Dynamic;


        var shape = new CircleShape
        (
            spawn.EntityData.Radius,
            spawn.EntityData.Density
        );

        body.CreateFixture(shape);
        body.Position = spawn.Position;

        _entities.Add(spawn.EntityID, body);
        Log.Information("SpawnEntityBody: ID = {id}, Pos = {pos}",
            spawn.EntityID, spawn.Position);
    }

    private void UpdatePositions()
    {
        foreach (var entity in _entities)
            if (entity.Value.BodyType == BodyType.Dynamic)
            {
                channelHub.PhysicsToMain.Writer.TryWrite(new PositionUpdate(entity.Key, entity.Value.Position));
                channelHub.PhysicsToLogic.Writer.TryWrite(new PositionUpdate(entity.Key, entity.Value.Position));
            }
    }
}