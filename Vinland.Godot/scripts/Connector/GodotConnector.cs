using System;
using System.Collections.Generic;
using Godot;
using Serilog;
using Vinland.Core.Connectors;
using Vinland.Core.Infrastructure.Bootstrap;
using Vinland.Core.Infrastructure.Channels;
using Vector2 = System.Numerics.Vector2;


namespace Vinland.Godot.Connector;

public class GodotConnector(IExternalModules modules, Node level) : Core.Connectors.Connector(modules)
{
    private Dictionary<long, CharacterBody2D> _entities = new();
    private readonly Dictionary<string, PackedScene> _sceneCache = new();
    
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
        }
    }
    
    private void SetVelocity(SetVelocity cmd)
    {
        if (!_entities.TryGetValue(cmd.EntityID, out var entity))
            Log.Warning("Entity {ID} not found, cannot set velocity", cmd.EntityID);
        else
        {
            entity.Velocity = cmd.Velocity.ToGodot();
            entity.MoveAndSlide();
        }
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

    protected override PositionSnapshot PositionSnapshot()
    {
        List<long> entities = new();
        List<Vector2> positions = new();

        foreach (var entity in _entities)
        {
            entities.Add(entity.Key);
            positions.Add(entity.Value.Position.ToNet());
        }
        
        return  new PositionSnapshot(entities, positions);
    }
}

