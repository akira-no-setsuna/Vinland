using System.Threading.Channels;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Input;

namespace Game.Core.Application.Input;

public class KbmInputSource(ChannelWriter<InputCommand> writer) : InputSource(writer)
{
    protected override InputCommand ReadInput()
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