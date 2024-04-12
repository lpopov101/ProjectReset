using System;
using Godot;

public partial class InputManager : Node
{
    [Export]
    public string _MoveForwardAction = "move_forward";

    [Export]
    public string _MoveBackwardAction = "move_backward";

    [Export]
    public string _MoveLeftAction = "move_left";

    [Export]
    public string _MoveRightAction = "move_right";

    [Export]
    public string _JumpAction = "jump";

    [Export]
    public string _InteractAction = "interact";

    [Export]
    public string _FireAction = "fire";

    [Export]
    public string _MenuAction = "menu";

    private Vector2 _mouseMotion = Vector2.Zero;

    public override void _Ready()
    {
        base._Ready();
        Locator<InputManager>.Register(this);
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseMotion eventMouseMotion)
        {
            _mouseMotion.X = eventMouseMotion.Relative.X;
            _mouseMotion.Y = eventMouseMotion.Relative.Y;
        }
    }

    public float GetVerticalMovementInput()
    {
        var forwardMovement = Input.IsActionPressed(_MoveForwardAction) ? 1F : 0F;
        var backwardMovement = Input.IsActionPressed(_MoveBackwardAction) ? 1F : 0F;
        return backwardMovement - forwardMovement;
    }

    public float GetHorizontalMovementInput()
    {
        var leftMovement = Input.IsActionPressed(_MoveLeftAction) ? 1F : 0F;
        var rightMovement = Input.IsActionPressed(_MoveRightAction) ? 1F : 0F;
        return rightMovement - leftMovement;
    }

    public bool GetJumpInput()
    {
        return Input.IsActionJustPressed(_JumpAction);
    }

    public bool GetInteractInput()
    {
        return Input.IsActionJustPressed(_InteractAction);
    }

    public string GetInteractKeys()
    {
        return getActionKeys(_InteractAction);
    }

    public bool GetFirePressedInput()
    {
        return Input.IsActionJustPressed(_FireAction);
    }

    public bool GetFireHeldInput()
    {
        return Input.IsActionPressed(_FireAction);
    }

    public bool GetMenuInput()
    {
        return Input.IsActionJustPressed(_MenuAction);
    }

    public Vector2 GetAndResetMouseMotion()
    {
        var result = _mouseMotion;
        _mouseMotion = Vector2.Zero;
        return result;
    }

    public void SetCursorCaptured()
    {
        Input.MouseMode = Input.MouseModeEnum.Captured;
    }

    public void SetCursorVisible()
    {
        Input.MouseMode = Input.MouseModeEnum.Visible;
    }

    private string getActionKeys(string action)
    {
        var result = "";
        foreach (var inputEvent in InputMap.ActionGetEvents(action))
        {
            if (result.Length > 0)
            {
                result += "/";
            }
            result += inputEvent.AsText().Split(' ')[0];
        }
        return result;
    }
}
