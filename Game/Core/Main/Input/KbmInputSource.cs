using System.Threading.Channels;
using Game.Core.Infrastructure.Channels;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Input;

namespace Game.Core.Main.Input;

public class KbmInputSource(ChannelHub channelHub) : InputSource(channelHub)
{
    private KeyboardStateExtended _currentKeyboard;
    private KeyboardStateExtended _previousKeyboard;
    
    private MouseStateExtended _currentMouse;
    private MouseStateExtended _previousMouse;

    protected override void UpdateDevice()
    {
        KeyboardExtended.Update();
        MouseExtended.Update();
        
        _currentKeyboard = KeyboardExtended.GetState();
        _currentMouse = MouseExtended.GetState();
    }

    protected override void ReadSnapshot()
    {
        InputSnapshot = InputSnapshot with
        {
            MoveUp = _currentKeyboard.IsKeyDown(Keys.W) || _currentKeyboard.IsKeyDown(Keys.Up),
            MoveDown = _currentKeyboard.IsKeyDown(Keys.S) || _currentKeyboard.IsKeyDown(Keys.Down),
            MoveLeft = _currentKeyboard.IsKeyDown(Keys.A) || _currentKeyboard.IsKeyDown(Keys.Left),
            MoveRight = _currentKeyboard.IsKeyDown(Keys.D) || _currentKeyboard.IsKeyDown(Keys.Right)
            
        };
    }

    protected override void ReadEvents()
    {
        CheckKey(
            InputAction.Attack, 
            _currentMouse.LeftButton == ButtonState.Pressed,
            _previousMouse.LeftButton == ButtonState.Pressed
            );
    }

    protected override void SavePreviousState()
    {
        _previousKeyboard = _currentKeyboard;
        _previousMouse = _currentMouse;
    }
    
    private void CheckKey(InputAction action, bool isDown, bool wasDown)
    {
        if (isDown && !wasDown)
            InputEvents.Enqueue(new InputEvent(action, InputEventType.Pressed));

        if (!isDown && wasDown)
            InputEvents.Enqueue(new InputEvent(action, InputEventType.Released));
    }
    
    
}