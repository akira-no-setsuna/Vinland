using System;
using System.Collections.Generic;
using Godot;
using Godot.Collections;
using Serilog;
using Vinland.Core.Infrastructure.Bootstrap;
using Vinland.Core.Infrastructure.Channels;
using Vector2 = System.Numerics.Vector2;

namespace Vinland.Godot.Connector;

public class GodotConnector(IExternalModules modules, Node2D level) : Core.Connectors.Connector(modules)
{
    private System.Collections.Generic.Dictionary<long, CharacterBody2D> _entities = new();
    private readonly System.Collections.Generic.Dictionary<string, PackedScene> _sceneCache = new();

    protected override void OnCommand(CoreCommand cmd)
    {
        switch (cmd)
        {
            case SetVelocity setVelocity:
                SetVelocity(setVelocity);
                break;
            case SpawnCommand spawn:
                Spawn(spawn);
                break;
            case RayCastRequest rayCast:
                HandleRayCasts(rayCast);
                break;
        }
    }

    private void SetVelocity(SetVelocity cmd)
    {
        if (cmd.EntityID.Count != cmd.Velocity.Count)
        {
            Log.Warning("IDs count({IDCount}) does doesnt up velocities count({VelocitiesCount})",
                cmd.EntityID.Count, cmd.Velocity.Count);
            return;
        }

        for (int i = 0; i < cmd.EntityID.Count; i++)
        {
            if (!_entities.TryGetValue(cmd.EntityID[i], out var entityBody)) continue;
            if (!GodotObject.IsInstanceValid(entityBody)) continue;

            entityBody.Velocity = cmd.Velocity[i].ToGodot();
            entityBody.MoveAndSlide();
        }
        
        cmd.Velocity.Clear();
    }

    private void Spawn(SpawnCommand cmd)
    {
        if (cmd.EntityID == null)
        {
            Log.Warning("Entity {ID} not found, cannot spawn", cmd.EntityID);
            return;
        }

        var kind = cmd.Kind.ToString().ToLowerInvariant();

        if (!_sceneCache.TryGetValue(kind, out var scene))
        {
            var path = $"res://scenes/{kind}.tscn";
            scene = GD.Load<PackedScene>(path);
            _sceneCache[kind] = scene;
        }
        var instance = scene.Instantiate<CharacterBody2D>();

        level.AddChild(instance);
        _entities.Add((long)cmd.EntityID, instance);
    }
    
    private void HandleRayCasts(RayCastRequest request)
    {
        var spaceState = level.GetWorld2D().DirectSpaceState;

        var results = new List<RayCastResult>(request.Requests.Count);

        foreach (var rc in request.Requests)
        {
            if (!_entities.TryGetValue(rc.Origin, out var originBody) ||
                !GodotObject.IsInstanceValid(originBody))
                continue;

            var from = originBody.GlobalPosition;
            var visible = new List<long>(rc.Targets.Count);

            foreach (var targetId in rc.Targets)
            {
                if (!_entities.TryGetValue(targetId, out var targetBody) ||
                    !GodotObject.IsInstanceValid(targetBody))
                    continue;

                var to = targetBody.GlobalPosition;
                    
                if (from.DistanceSquaredTo(to) < 1f)
                {
                    visible.Add(targetId);
                    continue;
                }

                var query = PhysicsRayQueryParameters2D.Create(from, to);
                
                query.Exclude = [originBody.GetRid()];
                query.CollideWithAreas  = false;
                query.CollideWithBodies = true;

                var hit = spaceState.IntersectRay(query);

                if (hit.Count == 0)
                {
                    visible.Add(targetId);
                }
                else if (hit["rid"].AsRid() == targetBody.GetRid())
                {
                    visible.Add(targetId);
                }
            }

            results.Add(new RayCastResult(rc.Origin, visible));
        }
        ChannelHub.Host.Writer.TryWrite(new RayCastResponse(results));
    }

    protected override PositionSnapshot PositionSnapshot()
    {
        List<long> entities = new();
        List<Vector2> positions = new();

        foreach (var entity in _entities)
        {
            entities.Add(entity.Key);
            positions.Add(entity.Value.Position.ToNet());
        }

        return new PositionSnapshot(entities, positions);
    }
}