using nkast.Aether.Physics2D.Dynamics;
using Vinland.Core.Input;
using Vector2Aether =  nkast.Aether.Physics2D.Common.Vector2;

namespace Game.Core.Entities;

public class PlayerController(Body body)
{
    private const float SPEED_METERS_PER_SECOND = 5f;

    public void FixedUpdate(InputCommand input)
    {
        Vector2Aether velocity = Vector2Aether.Zero;
        
        if (input.MoveUp) velocity.Y -= 1;
        if (input.MoveDown) velocity.Y += 1;
        if (input.MoveLeft) velocity.X -= 1;
        if (input.MoveRight) velocity.X += 1;

        if (velocity.LengthSquared() > 0)
        {
            velocity.Normalize();
            velocity *= SPEED_METERS_PER_SECOND;
        }
        body.LinearVelocity = velocity;
    }
}