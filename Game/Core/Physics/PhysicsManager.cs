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
    private readonly MapColliderGenerator _mapColliderGenerator = new();
    private readonly World _physicWorld = new(Vector2.Zero);

    private readonly List<PositionSnapshot> _positionBuffer = new();

    protected override void FixedUpdate(long tick, float deltaTime)
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
        channelHub.PhysicsToMain.Writer.TryWrite(new BodyListRender(CurrentTick, _physicWorld.BodyList));
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
            spawn.Radius,
            spawn.Density
        );

        body.CreateFixture(shape);
        body.Position = spawn.Position;

        _entities.Add(spawn.EntityID, body);
        Log.Information("SpawnEntityBody: ID = {id}, Pos = {pos}",
            spawn.EntityID, spawn.Position);
    }

    private void UpdatePositions()
    {
        _positionBuffer.Clear();

        if (_entities.Count > 0)
            _positionBuffer.EnsureCapacity(_entities.Count);

        foreach (var entity in _entities)
        {
            if (entity.Value.BodyType != BodyType.Dynamic)
                continue;

            _positionBuffer.Add(
                new PositionSnapshot(
                    entity.Key,
                    entity.Value.Position
                )
            );
        }

        if (_positionBuffer.Count == 0)
            return;

        var positionSnapshots = _positionBuffer.ToArray();
        channelHub.PhysicsToMain.Writer.TryWrite(new PositionsUpdate(CurrentTick, positionSnapshots));
        channelHub.PhysicsToLogic.Writer.TryWrite(new PositionsUpdate(CurrentTick, positionSnapshots));
    }
}