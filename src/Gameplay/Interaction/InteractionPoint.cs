using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using Godot;

[GlobalClass]
public partial class InteractionPoint : Node3D
{
    enum State
    {
        Hidden,
        Hint,
        MaybeInteractable,
        Interactable
    }

    [Signal]
    public delegate void OnInteractEventHandler();

    public delegate void InteractionPointEventHandler(InteractionPoint interactionPoint);

    public static event InteractionPointEventHandler VisibleInteractionPointAdded;
    public static event InteractionPointEventHandler VisibleInteractionPointRemoved;

    private static InteractionPoint CurInteractablePoint;

    [Export]
    private string _InteractionPrompt = "Interact";

    [Export]
    private float _MaxInteractionHintDistance = 50F;

    [Export]
    private float _MaxInteractionDistance = 5F;

    [Export]
    private bool _IgnoreObstructions = true;

    private StateMachine<State> _stateMachine;
    private Camera3D _camera;
    private float _distanceFromCamera = 0F;
    private Vector2 _screenCoords = Vector2.Zero;
    private bool _enabled = true;

    public override void _Ready()
    {
        base._Ready();
        _stateMachine = new StateMachine<State>(State.Hidden);
        _camera = Locator<PlayerManager>.Get().Player1().GetCamera();

        _stateMachine.addStateTransition(
            State.Hidden,
            State.Hint,
            () =>
            {
                return !_camera.IsPositionBehind(GlobalPosition)
                    && _distanceFromCamera < _MaxInteractionHintDistance
                    && _enabled;
            }
        );

        _stateMachine.addStateTransition(
            State.Hint,
            State.MaybeInteractable,
            () =>
            {
                return _distanceFromCamera < _MaxInteractionDistance && IsUnobstructedFromCamera();
            }
        );

        _stateMachine.addStateTransition(
            State.MaybeInteractable,
            State.Interactable,
            () =>
            {
                return CurInteractablePoint == null
                    || GetProximityScore() > CurInteractablePoint.GetProximityScore();
            }
        );

        _stateMachine.addStateTransition(
            State.Interactable,
            State.MaybeInteractable,
            () =>
            {
                return !IsCurInteractablePoint();
            }
        );

        _stateMachine.addStateTransitions(
            new State[] { State.Interactable, State.MaybeInteractable },
            State.Hint,
            () =>
            {
                return _distanceFromCamera > _MaxInteractionDistance || !IsUnobstructedFromCamera();
            }
        );

        _stateMachine.addStateTransitions(
            new State[] { State.Interactable, State.MaybeInteractable, State.Hint },
            State.Hidden,
            () =>
            {
                return _camera.IsPositionBehind(GlobalPosition)
                    || _distanceFromCamera > _MaxInteractionHintDistance
                    || !_enabled;
            }
        );

        _stateMachine.addStateEnterAction(
            State.Hidden,
            () =>
            {
                VisibleInteractionPointRemoved.Invoke(this);
            }
        );

        _stateMachine.addStateExitAction(
            State.Hidden,
            () =>
            {
                VisibleInteractionPointAdded.Invoke(this);
            }
        );

        _stateMachine.addStateEnterAction(
            State.Interactable,
            () =>
            {
                CurInteractablePoint = this;
            }
        );

        _stateMachine.addStateExitAction(
            State.Interactable,
            () =>
            {
                if (IsCurInteractablePoint())
                {
                    CurInteractablePoint = null;
                }
            }
        );

        _stateMachine.addStateProcessAction(
            State.Interactable,
            () =>
            {
                if (Locator<InputManager>.Get().GetInteractInput())
                {
                    Locator<MessageManager>.Get().AddMessage("Interacted with " + Name);
                    EmitSignal(SignalName.OnInteract);
                }
            }
        );
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        _distanceFromCamera = GlobalPosition.DistanceTo(_camera.GlobalPosition);
        _screenCoords = _camera.UnprojectPosition(GlobalPosition);

        _stateMachine.ProcessState();
    }

    public void SetPrompt(string interactionPrompt)
    {
        _InteractionPrompt = interactionPrompt;
    }

    public void SetEnabled(bool enabled)
    {
        _enabled = enabled;
    }

    public Vector2 GetScreenCoords()
    {
        return _screenCoords;
    }

    public string GetInteractionPrompt()
    {
        return _InteractionPrompt;
    }

    public float GetNormalizedDistanceToCamera()
    {
        var unclamped = Mathf.InverseLerp(
            _MaxInteractionHintDistance,
            _MaxInteractionDistance,
            _distanceFromCamera
        );
        return Mathf.Clamp(unclamped, 0, 1F);
    }

    private float GetNormalizedDistanceToScreenCenter()
    {
        var viewportSize = Locator<UIBase>.Get().GetViewportRect().Size;
        var xAxisLength = viewportSize.X / 2F;
        var yAxisLength = viewportSize.Y / 2F;
        var xNormalized = (_screenCoords.X - xAxisLength) / xAxisLength;
        var yNormalized = (_screenCoords.Y - yAxisLength) / yAxisLength;
        return new Vector2(xNormalized, yNormalized).Length() / Mathf.Sqrt2;
    }

    public float GetProximityScore()
    {
        return 1F
            - ((GetNormalizedDistanceToCamera() + GetNormalizedDistanceToScreenCenter()) / 2F);
    }

    public bool IsCurInteractablePoint()
    {
        return this == CurInteractablePoint;
    }

    public static InteractionPoint GetCurInteractablePoint()
    {
        return CurInteractablePoint;
    }

    private bool IsUnobstructedFromCamera()
    {
        if (_IgnoreObstructions)
        {
            return true;
        }
        var directionFromCamera = (GlobalPosition - _camera.GlobalPosition).Normalized();
        var hit = new RaycastBuilder(this)
            .FromPosition(_camera.GlobalPosition)
            .WithDirectionAndMagnitude(directionFromCamera, _distanceFromCamera + 2F)
            .WithIgnoredObject(Locator<PlayerManager>.Get().Player1().GetCharacterBody())
            .Cast();
        if (hit != null && _camera.GlobalPosition.DistanceTo(hit.Position) < _distanceFromCamera)
        {
            return false;
        }
        return true;
    }
}
