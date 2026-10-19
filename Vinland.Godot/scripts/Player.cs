using Godot;
using Vinland.Monogame;
using Vinland.Monogame.Core.Logic;

namespace Vinland.Godot;

public partial class Player : CharacterBody2D
{
    private float _speed = 10f;
    private const uint PPM = 16;

    public override void _PhysicsProcess(double delta)
    {
        Velocity = MovementVector() * _speed * PPM;
        MoveAndSlide();
    }

    private Vector2 MovementVector()
    {
        return Input.GetVector(
            "move_left", 
            "move_right",
            "move_up",
            "move_down"
            );
    }
}