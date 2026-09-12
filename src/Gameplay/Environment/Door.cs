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

    private enum StateMachineEvents
    {
        Open,
        Close
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
    private Vector3 _openerPosition;
    private Godot.Collections.Array<Node3D> _touchingBodies = new Godot.Collections.Array<Node3D>();

    public override void _Ready()
    {
        base._Ready();
        _stateMachine = new StateMachine<State>(State.Closed);
        _autoCloseTimer = new Timer();
        _panel = GetNode<RigidBody3D>($"{GetPath()}/Panel");
        _panel.SetCollisionLayerValue(PlayerManager.PLAYER_COLLISION_LAYER, true);
        _panel.SetCollisionMaskValue(PlayerManager.PLAYER_COLLISION_LAYER, true);
        // Contacts are how we notice the panel has swung into someone, and a sleeping panel
        // stops reporting them while it rests against them.
        _panel.CanSleep = false;
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

        _stateMachine.addStateTransition(State.Closed, State.Open, (int)StateMachineEvents.Open);
        _stateMachine.addStateTransition(State.Open, State.Closing, (int)StateMachineEvents.Close);
        _stateMachine.addStateTransition(
            State.Open,
            State.Closing,
            () =>
            {
                return AnyTouchingCloser();
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
                    + (
                        (CloserToFront(_openerPosition) ? 1F : -1F)
                        * Mathf.DegToRad(_AutoOpenDegrees)
                    );
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

        _touchingBodies = _panel.GetCollidingBodies();
        _stateMachine.ProcessState(delta);
    }

    // Opens whichever door the character walked into, if it is allowed to open doors.
    // Detection has to come from the character's own slide collisions: a closed panel is
    // hinge-locked, so a character pressing against it never penetrates it and the panel
    // reports no contacts of its own.
    public static void OpenTouchedDoors(CharacterBody3D character)
    {
        if (character is not IDoorOpener opener || !opener.CanOpenDoors())
        {
            return;
        }
        for (int i = 0; i < character.GetSlideCollisionCount(); i++)
        {
            var node = character.GetSlideCollision(i).GetCollider() as Node;
            while (node != null && node is not Door)
            {
                node = node.GetParent();
            }
            if (node is Door door)
            {
                door.RequestOpen(character.GlobalPosition);
                return;
            }
        }
    }

    private bool AnyTouchingCloser()
    {
        foreach (var body in _touchingBodies)
        {
            if (body is IDoorCloser closer && closer.CanCloseDoors())
            {
                return true;
            }
        }
        return false;
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
        return CloserToFront(GetPlayerPosition());
    }

    private bool CloserToFront(Vector3 position)
    {
        var frontDistance = position.DistanceTo(_frontInteractionPoint.GlobalPosition);
        var backDistance = position.DistanceTo(_backInteractionPoint.GlobalPosition);
        return frontDistance < backDistance;
    }

    private Vector3 GetPlayerPosition()
    {
        return Locator<PlayerManager>.Get().Player1().GetCharacterBody().GlobalPosition;
    }

    private void SignalOpen()
    {
        RequestOpen(GetPlayerPosition());
    }

    private void RequestOpen(Vector3 openerPosition)
    {
        _openerPosition = openerPosition;
        _stateMachine.SendEvent((int)StateMachineEvents.Open);
    }

    private void SignalClose()
    {
        _stateMachine.SendEvent((int)StateMachineEvents.Close);
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

    private bool IsFullyOpen()
    {
        return Mathf.Abs(_panel.Rotation.Y)
            > Mathf.DegToRad(Mathf.Abs(_OpenDegreesLimit)) - ANGLE_EPSILON;
    }

    private void MoveTowardsTarget()
    {
        var fullyOpen = IsFullyOpen();
        // Prevent characters from clipping through door if all the way open
        _panel.SetCollisionLayerValue(PlayerManager.PLAYER_COLLISION_LAYER, fullyOpen);
        var reachedTarget = Mathf.Abs(_panel.Rotation.Y - _targetYRotation) < ANGLE_EPSILON;
        if ((fullyOpen && _touchingBodies.Count > 0) || reachedTarget)
        {
            _panel.AngularVelocity = Vector3.Zero;
            return;
        }
        var openSpeed = (_targetYRotation - _panel.Rotation.Y) * _OpenForce;
        _panel.AngularVelocity = new Vector3(0, openSpeed, 0);
    }
}
