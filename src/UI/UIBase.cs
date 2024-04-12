using System;
using Godot;

public partial class UIBase : Control
{
    enum State
    {
        InGame,
        InGameMenu
    }

    [Export]
    private Control _MenuUI;

    [Export]
    private Control _HUDUI;

    [Export]
    private InventoryPanel _InventoryPanel;

    private StateMachine<State> _stateMachine;

    public override void _Ready()
    {
        base._Ready();
        Locator<UIBase>.Register(this);

        _InventoryPanel.SetInventory(Locator<PlayerManager>.Get().Player1().GetInventory());
        Locator<InputManager>.Get().SetCursorCaptured();
        _MenuUI.Visible = false;
        _stateMachine = new StateMachine<State>(State.InGame);

        _stateMachine.addStateTransition(
            State.InGame,
            State.InGameMenu,
            () =>
            {
                return Locator<InputManager>.Get().GetMenuInput();
            }
        );

        _stateMachine.addStateTransition(
            State.InGameMenu,
            State.InGame,
            () =>
            {
                return Locator<InputManager>.Get().GetMenuInput();
            }
        );

        _stateMachine.addStateEnterAction(
            State.InGame,
            () =>
            {
                _HUDUI.Visible = true;
                Locator<InputManager>.Get().SetCursorCaptured();
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
                Locator<InputManager>.Get().SetCursorVisible();
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
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        _stateMachine.ProcessState();
    }
}
