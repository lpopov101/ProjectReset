using System;
using Godot;

public partial class UIBase : Control
{
    enum State
    {
        InGame,
        InGameMenu
    }

    private static UIBase _instance;

    [Export]
    private Control _MenuUI;

    [Export]
    private Control _HUDUI;

    private StateMachine<State> _stateMachine;

    public override void _Ready()
    {
        base._Ready();

        GameManager.InputManager().SetCursorCaptured();
        _MenuUI.Visible = false;
        _stateMachine = new StateMachine<State>(State.InGame);

        _stateMachine.addStateTransition(
            State.InGame,
            State.InGameMenu,
            () =>
            {
                return GameManager.InputManager().GetMenuInput();
            }
        );

        _stateMachine.addStateTransition(
            State.InGameMenu,
            State.InGame,
            () =>
            {
                return GameManager.InputManager().GetMenuInput();
            }
        );

        _stateMachine.addStateEnterAction(
            State.InGame,
            () =>
            {
                _HUDUI.Visible = true;
                GameManager.InputManager().SetCursorCaptured();
            }
        );

        _stateMachine.addStateExitAction(
            State.InGame,
            () =>
            {
                _HUDUI.Visible = false;
            }
        );

        _stateMachine.addStateEnterAction(
            State.InGameMenu,
            () =>
            {
                GetTree().Paused = true;
                _MenuUI.Visible = true;
                GameManager.InputManager().SetCursorVisible();
            }
        );

        _stateMachine.addStateExitAction(
            State.InGameMenu,
            () =>
            {
                GetTree().Paused = false;
                _MenuUI.Visible = false;
            }
        );

        _instance = this;
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        _stateMachine.ProcessState();
    }

    public static UIBase Instance()
    {
        return _instance;
    }
}
