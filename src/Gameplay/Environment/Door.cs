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
    private const float ANGLE_EPSILON = 0.05F;

    [Export]
    private float _DoorOpenDegrees = 20F;

    [Export]
    private float _DoorMotorSpeed = 80F;

    private StateMachine<State> _stateMachine;

    private RigidBody3D _panel;
    private HingeJoint3D _hingeJoint;
    private InteractionPoint _frontInteractionPoint;
    private InteractionPoint _backInteractionPoint;
    private bool _touchingPlayer = false;
    private double _targetYRotation = 0.0;

    public override void _Ready()
    {
        base._Ready();
        _stateMachine = new StateMachine<State>(State.Closed);
        _panel = GetNode<RigidBody3D>($"{GetPath()}/Panel");
        _panel.SetCollisionLayerValue(PlayerManager.PLAYER_COLLISION_LAYER, true);
        _panel.SetCollisionMaskValue(PlayerManager.PLAYER_COLLISION_LAYER, true);
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
                return Mathf.Abs(_panel.Rotation.Y) < ANGLE_EPSILON;
            }
        );

        _stateMachine.addStateEnterAction(State.Closed, Lock);
        _stateMachine.addStateProcessAction(State.Closed, SelectInteractionPoint);
        _stateMachine.addStateEnterAction(
            State.Open,
            () =>
            {
                Unlock();
                _targetYRotation =
                    (PlayerCloserToFront() ? 1F : -1F) * Mathf.DegToRad(_DoorOpenDegrees);
            }
        );
        _stateMachine.addStateProcessAction(State.Open, SetHingeMotorToTarget);
        _stateMachine.addStateEnterAction(
            State.Closing,
            () =>
            {
                _targetYRotation = 0F;
            }
        );
        _stateMachine.addStateProcessAction(State.Closing, SetHingeMotorToTarget);
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

    private void Lock()
    {
        _hingeJoint.SetFlag(HingeJoint3D.Flag.EnableMotor, false);
        _hingeJoint.SetParam(HingeJoint3D.Param.LimitLower, 0.0F);
        _hingeJoint.SetParam(HingeJoint3D.Param.LimitUpper, 0.0F);
        _panel.SetCollisionLayerValue(PlayerManager.PLAYER_COLLISION_LAYER, true);
        _frontInteractionPoint.SetEnabled(true);
        _backInteractionPoint.SetEnabled(true);
    }

    private void Unlock()
    {
        _hingeJoint.SetFlag(HingeJoint3D.Flag.EnableMotor, true);
        _hingeJoint.SetParam(HingeJoint3D.Param.LimitLower, Mathf.DegToRad(-90.0F));
        _hingeJoint.SetParam(HingeJoint3D.Param.LimitUpper, Mathf.DegToRad(90.0F));
        _panel.SetCollisionLayerValue(PlayerManager.PLAYER_COLLISION_LAYER, false);
        _frontInteractionPoint.SetEnabled(false);
        _backInteractionPoint.SetEnabled(false);
    }

    private void SetHingeMotorToTarget()
    {
        if (Mathf.Abs(_panel.Rotation.Y - _targetYRotation) < ANGLE_EPSILON)
        {
            _hingeJoint.SetParam(HingeJoint3D.Param.MotorTargetVelocity, 0.0F);
            return;
        }
        if (_panel.Rotation.Y > _targetYRotation)
        {
            _hingeJoint.SetParam(
                HingeJoint3D.Param.MotorTargetVelocity,
                Mathf.DegToRad(_DoorMotorSpeed)
            );
        }
        else
        {
            _hingeJoint.SetParam(
                HingeJoint3D.Param.MotorTargetVelocity,
                -Mathf.DegToRad(_DoorMotorSpeed)
            );
        }
    }
}
