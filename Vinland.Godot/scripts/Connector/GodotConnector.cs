using Godot;
using Vinland.Core.Infrastructure.Bootstrap;
using Vinland.Core.Infrastructure.Channels;

namespace Vinland.Godot.Connector;

public class GodotConnector(IExternalModules modules, Node level) : Core.Connectors.Connector(modules)
{
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
        
    }
    
    private void Spawn(SpawnCommand cmd)
    {
        var scene = GD.Load<PackedScene>($"res://scenes/player.tscn");
        var entity = scene.Instantiate<CharacterBody2D>(); 
        entity.Position = cmd.Position.ToGodot();
        level.AddChild(entity);
    }
}

