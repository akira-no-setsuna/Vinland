using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Input;

namespace Game.Core.Input;

public class KbmInputSource : IInputSource
{
    public InputCommand ReadInput()
    {
        var keyboardState = KeyboardExtended.GetState();
        // var keyboardState = Keyboard.GetState();
        var mouseState = MouseExtended.GetState();

        return new InputCommand
        (
            MoveUp: keyboardState.IsKeyDown(Keys.W) || keyboardState.IsKeyDown(Keys.Up),
            MoveDown: keyboardState.IsKeyDown(Keys.S) || keyboardState.IsKeyDown(Keys.Down),
            MoveLeft: keyboardState.IsKeyDown(Keys.A) || keyboardState.IsKeyDown(Keys.Left),
            MoveRight: keyboardState.IsKeyDown(Keys.D) || keyboardState.IsKeyDown(Keys.Right),
            
            Attack: mouseState.LeftButton == ButtonState.Pressed
        );
    }
}