using System;
using System.Collections.Generic;
using Godot;
using Godot.Collections;
using Serilog;
using Vinland.Core.Infrastructure.Bootstrap;
using Vinland.Core.Infrastructure.Channels;
using Vector2 = System.Numerics.Vector2;
using Vector2GD = Godot.Vector2;

namespace Vinland.Godot.Connector;

public class GodotConnector : Core.Connectors.Connector
{
    private readonly Node2D _level;
    
    private readonly System.Collections.Generic.Dictionary<long, CharacterBody2D> _entitiesBody;
    private readonly System.Collections.Generic.Dictionary<string, PackedScene> _sceneCache;
    
    private readonly PhysicsManager _physicsManager;


    public GodotConnector(IExternalModules modules, Node2D level) : base(modules)
    {
        _level = level;
        
        _entitiesBody = new();
        _sceneCache = new();
        
        _physicsManager = new (_level, _entitiesBody);
        
    }

    protected override void FixedUpdate(float deltaTime)
    {
        foreach (var entry in _entitiesBody)
        {
            var body = entry.Value;
            var agent = body.GetNodeOrNull<NavigationAgent2D>("NavigationAgent2D");

            if (agent != null && !agent.IsNavigationFinished())
            {
                Vector2GD nextPathPosition = agent.GetNextPathPosition();
                Vector2GD direction = (nextPathPosition - body.GlobalPosition).Normalized();
                
                body.Velocity = direction * 16.0f; 
            }

            body.MoveAndSlide();
        }
    }

    protected override void OnCommand(CoreCommand cmd)
    {
        switch (cmd)
        {
            case SetVelocity setVelocity:
                _physicsManager.SetVelocity(setVelocity);
                break;
            case SpawnCommand spawn:
                Spawn(spawn);
                break;
            case RayCastRequest rayCast:
                ChannelHub.Host.Writer.TryWrite(_physicsManager.HandleRayCasts(rayCast));
                break;
            case SetTarget setTarget:
                AgentFollow(setTarget);
                break;
        }
    }

    private void AgentFollow(SetTarget setTarget)
    {
        if (!_entitiesBody.TryGetValue(setTarget.AgentID, out var body)) return;
    
        var agent = body.GetNodeOrNull<NavigationAgent2D>("NavigationAgent2D");
        if (agent == null) return;

        agent.TargetPosition = _entitiesBody[setTarget.TargetID].GlobalPosition;
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
        instance.Position = cmd.Position.ToGodot();
        
        _level.AddChild(instance);
        _entitiesBody.Add((long)cmd.EntityID, instance);
    }

    protected override PositionSnapshot PositionSnapshot()
    {
        List<long> entities = new();
        List<Vector2> positions = new();

        foreach (var entity in _entitiesBody)
        {
            entities.Add(entity.Key);
            positions.Add(entity.Value.Position.ToNet());
        }

        return new PositionSnapshot(entities, positions);
    }
}