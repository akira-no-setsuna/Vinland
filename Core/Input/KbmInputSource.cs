using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Input;

namespace Vinland.Core.Input;

public class KbmInputSource : IInputSource
{
    public InputCommand ReadInput()
    {
        var keyboardState = KeyboardExtended.GetState();
        // var keyboardState = Keyboard.GetState();
        var mouseState = MouseExtended.GetState();

        return new InputCommand
        (
            moveUp: keyboardState.IsKeyDown(Keys.W) || keyboardState.IsKeyDown(Keys.Up),
            moveDown: keyboardState.IsKeyDown(Keys.S) || keyboardState.IsKeyDown(Keys.Down),
            moveLeft: keyboardState.IsKeyDown(Keys.A) || keyboardState.IsKeyDown(Keys.Left),
            moveRight: keyboardState.IsKeyDown(Keys.D) || keyboardState.IsKeyDown(Keys.Right),
            
            attack: mouseState.LeftButton == ButtonState.Pressed
        );
    }
}