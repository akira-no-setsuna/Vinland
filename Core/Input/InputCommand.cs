namespace Vinland.Core.Input;

public readonly struct InputCommand
{
    // Moving
    // TODO: Convert movement to Vector2 
    public readonly bool MoveLeft;
    public readonly bool MoveRight;
    public readonly bool MoveUp;
    public readonly bool MoveDown;
    
    //Attacking
    public readonly bool Attack;

    public InputCommand(
        bool moveLeft,
        bool moveRight,
        bool moveUp, 
        bool moveDown, 
        bool attack
        )
    {
        MoveLeft = moveLeft;
        MoveRight = moveRight;
        MoveUp = moveUp;
        MoveDown = moveDown;
        
        Attack = attack;
    }
}

public class InputCommandBuffer
{
    private InputCommand _current;
    private InputCommand _next;
    
    public InputCommand Current => _current;

    public void RecordForCurrentFrame(InputCommand command)
    {
        _next = command;
    }
    
    public void AdvanceFrame()
    {
        _current = _next;
    }
}

public interface IInputSource
{
    InputCommand ReadInput();
}
