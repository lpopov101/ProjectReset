using System;
using Godot;

public partial class Door : Node3D
{
    private enum State
    {
        Closed,
        Open,
        Closing
    }

    private const string OPEN_EVENT_NAME = "Open";
    private const string CLOSE_EVENT_NAME = "Close";
    private const float ANGLE_EPSILON = 0.1F;

    [Export]
    private float _AutoOpenDegrees = 20F;

    [Export]
    private float _OpenDegreesLimit = 90F;

    [Export]
    private float _OpenForce = 10F;

    [Export]
    private float _AutoCloseTime = 5F;

    private StateMachine<State> _stateMachine;

    private RigidBody3D _panel;
    private HingeJoint3D _hingeJoint;
    private InteractionPoint _frontInteractionPoint;
    private InteractionPoint _backInteractionPoint;
    private Timer _autoCloseTimer;
    private float _initYRotation;
    private float _targetYRotation;
    private bool _touchingPlayer = false;

    public override void _Ready()
    {
        base._Ready();
        _stateMachine = new StateMachine<State>(State.Closed);
        _autoCloseTimer = new Timer();
        _panel = GetNode<RigidBody3D>($"{GetPath()}/Panel");
        _panel.SetCollisionLayerValue(PlayerManager.PLAYER_COLLISION_LAYER, true);
        _panel.SetCollisionMaskValue(PlayerManager.PLAYER_COLLISION_LAYER, true);
        _initYRotation = _panel.Rotation.Y;
        _targetYRotation = _initYRotation;
        _hingeJoint = GetNode<HingeJoint3D>($"{GetPath()}/Hinge");

        _frontInteractionPoint = GetNode<InteractionPoint>(
            $"{_panel.GetPath()}/FrontInteractionPoint"
        );
        if (_frontInteractionPoint == null)
        {
            _frontInteractionPoint = new InteractionPoint();
            _frontInteractionPoint.SetPrompt("Open");
            _panel.AddChild(_frontInteractionPoint);
        }
        _frontInteractionPoint.Connect(
            InteractionPoint.SignalName.OnInteract,
            new Callable(this, nameof(SignalOpen))
        );

        _backInteractionPoint = GetNode<InteractionPoint>(
            $"{_panel.GetPath()}/BackInteractionPoint"
        );
        if (_backInteractionPoint == null)
        {
            _backInteractionPoint = new InteractionPoint();
            _backInteractionPoint.SetPrompt("Open");
            _panel.AddChild(_backInteractionPoint);
        }
        _backInteractionPoint.Connect(
            InteractionPoint.SignalName.OnInteract,
            new Callable(this, nameof(SignalOpen))
        );

        _autoCloseTimer = new Timer();
        _autoCloseTimer.OneShot = true;
        AddChild(_autoCloseTimer);
        _autoCloseTimer.Connect(Timer.SignalName.Timeout, new Callable(this, nameof(SignalClose)));

        _panel.BodyEntered += (body) =>
        {
            if (body is IPlayer)
            {
                _touchingPlayer = true;
            }
        };

        _panel.BodyExited += (body) =>
        {
            if (body is IPlayer)
            {
                _touchingPlayer = false;
            }
        };

        _stateMachine.addStateTransition(State.Closed, State.Open, OPEN_EVENT_NAME);
        _stateMachine.addStateTransition(State.Open, State.Closing, CLOSE_EVENT_NAME);
        _stateMachine.addStateTransition(
            State.Open,
            State.Closing,
            () =>
            {
                return _touchingPlayer;
            }
        );
        _stateMachine.addStateTransition(
            State.Closing,
            State.Closed,
            () =>
            {
                return Mathf.Abs(_panel.Rotation.Y - _initYRotation) < ANGLE_EPSILON;
            }
        );

        _stateMachine.addStateEnterAction(
            State.Open,
            () =>
            {
                Unlock();
                _autoCloseTimer.Start(_AutoCloseTime);
                _targetYRotation =
                    _initYRotation
                    + ((PlayerCloserToFront() ? 1F : -1F) * Mathf.DegToRad(_AutoOpenDegrees));
            }
        );
        _stateMachine.addStateProcessAction(State.Open, MoveTowardsTarget);
        _stateMachine.addStateEnterAction(
            State.Closing,
            () =>
            {
                _targetYRotation = _initYRotation;
            }
        );
        _stateMachine.addStateProcessAction(State.Closing, MoveTowardsTarget);
        _stateMachine.addStateEnterAction(State.Closed, Lock);
        _stateMachine.addStateProcessAction(State.Closed, SelectInteractionPoint);
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        _stateMachine.ProcessState();
    }

    private void SelectInteractionPoint()
    {
        if (PlayerCloserToFront())
        {
            _frontInteractionPoint.SetEnabled(true);
            _backInteractionPoint.SetEnabled(false);
        }
        else
        {
            _frontInteractionPoint.SetEnabled(false);
            _backInteractionPoint.SetEnabled(true);
        }
    }

    private bool PlayerCloserToFront()
    {
        var playerPosition = Locator<PlayerManager>
            .Get()
            .Player1()
            .GetCharacterBody()
            .GlobalPosition;
        var frontDistance = playerPosition.DistanceTo(_frontInteractionPoint.GlobalPosition);
        var backDistance = playerPosition.DistanceTo(_backInteractionPoint.GlobalPosition);
        return frontDistance < backDistance;
    }

    private void SignalOpen()
    {
        _stateMachine.SendEvent(OPEN_EVENT_NAME);
    }

    private void SignalClose()
    {
        _stateMachine.SendEvent(CLOSE_EVENT_NAME);
    }

    private void Lock()
    {
        _hingeJoint.SetParam(HingeJoint3D.Param.LimitLower, 0.0F);
        _hingeJoint.SetParam(HingeJoint3D.Param.LimitUpper, 0.0F);
        _panel.SetCollisionLayerValue(PlayerManager.PLAYER_COLLISION_LAYER, true);
        _frontInteractionPoint.SetEnabled(true);
        _backInteractionPoint.SetEnabled(true);
    }

    private void Unlock()
    {
        _hingeJoint.SetParam(
            HingeJoint3D.Param.LimitLower,
            _initYRotation - Mathf.DegToRad(Mathf.Abs(_OpenDegreesLimit))
        );
        _hingeJoint.SetParam(
            HingeJoint3D.Param.LimitUpper,
            _initYRotation + Mathf.DegToRad(Mathf.Abs(_OpenDegreesLimit))
        );
        _panel.SetCollisionLayerValue(PlayerManager.PLAYER_COLLISION_LAYER, false);
        _frontInteractionPoint.SetEnabled(false);
        _backInteractionPoint.SetEnabled(false);
    }

    private void MoveTowardsTarget()
    {
        var fullyOpen =
            Mathf.Abs(_panel.Rotation.Y)
            > Mathf.DegToRad(Mathf.Abs(_OpenDegreesLimit)) - ANGLE_EPSILON;
        // Prevent player from clipping through door if all the way open
        _panel.SetCollisionLayerValue(PlayerManager.PLAYER_COLLISION_LAYER, fullyOpen);
        var reachedTarget = Mathf.Abs(_panel.Rotation.Y - _targetYRotation) < ANGLE_EPSILON;
        if ((fullyOpen && _touchingPlayer) || reachedTarget)
        {
            _panel.AngularVelocity = Vector3.Zero;
            return;
        }
        var openSpeed = (_targetYRotation - _panel.Rotation.Y) * _OpenForce;
        _panel.AngularVelocity = new Vector3(0, openSpeed, 0);
    }
}
