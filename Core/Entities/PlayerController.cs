using nkast.Aether.Physics2D.Dynamics;
using Vector2Aether =  nkast.Aether.Physics2D.Common.Vector2;
using Vinland.Core.Input;

namespace Vinland.Core.Entities;

public class PlayerController(Body body)
{
    private readonly Body _body = body;
    private const float SpeedMetersPerSecond = 50f;

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
            velocity *= SpeedMetersPerSecond;
        }
        _body.LinearVelocity = velocity;
    }
}