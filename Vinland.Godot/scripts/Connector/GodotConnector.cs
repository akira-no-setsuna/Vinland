using System;
using System.Collections.Generic;
using Godot;
using Serilog;
using Vinland.Core.Infrastructure.Bootstrap;
using Vinland.Core.Infrastructure.Channels;

namespace Vinland.Godot.Connector;

public class GodotConnector(IExternalModules modules, Node level) : Core.Connectors.Connector(modules)
{
    private Dictionary<Guid, CharacterBody2D> _entities = new ();
    
    protected override void OnCommand(GameplayCommand cmd)
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
            Log.Warning($"Entity {cmd.EntityID} not found, cannot set velocity");
        else
        {
            entity.Velocity = cmd.Velocity.ToGodot();
            entity.MoveAndSlide();
        }


    }
    
    private void Spawn(SpawnCommand cmd)
    {
        var scene = GD.Load<PackedScene>($"res://scenes/player.tscn");
        var entity = scene.Instantiate<CharacterBody2D>(); 
        _entities.Add(cmd.EntityID, entity);
        
        entity.Position = cmd.Position.ToGodot();
        level.AddChild(entity);
    }
}

