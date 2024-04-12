using System;
using System.Runtime.InteropServices;
using System.Text;
using Godot;

[GlobalClass]
public partial class FirstPersonController : CharacterBody3D, IPickupable, IPlayer
{
    enum State
    {
        Grounded,
        Airborne,
    }

    [Export]
    private float _MouseSensitivity = 0.2f;

    [Export]
    private float _GroundedMovementSpeed = 5.0f;

    [Export]
    private float _GroundedFriction = 15.0F;

    [Export]
    private float _MaxHorizontalSpeed = 6.0f;

    [Export]
    private float _AirborneAcceleration = 100.0F;

    [Export]
    private float _Gravity = -9.8f;

    [Export]
    private float _JumpForce = 4.5f;

    [Export]
    private float _MaxStairHeight = 0.1f;

    [Export]
    private Node3D _StairProbe;

    [Export]
    private float _StairProbeDistance = 0.5F;

    private StateMachine<State> _stateMachine;
    private Camera3D _camera;
    private float _pitch = 0.0f;
    private float _curDelta = 0.0F;

    public override void _Ready()
    {
        _stateMachine = new StateMachine<State>(State.Airborne);
        _camera = GetViewport().GetCamera3D();
        FloorSnapLength = _MaxStairHeight + 0.1F;

        _stateMachine.addStateProcessAction(
            State.Grounded,
            () =>
            {
                var direction = getMovementDirection();
                var jumpForce = Locator<InputManager>.Get().GetJumpInput() ? _JumpForce : 0F;
                var targetVelocity = VelocityBuilder
                    .FromVelocity(Velocity)
                    .WithGroundedMovement(
                        direction,
                        _GroundedMovementSpeed,
                        _GroundedFriction,
                        _curDelta
                    )
                    .WithClampedHorizontalSpeed(_MaxHorizontalSpeed)
                    .WithJumping(jumpForce)
                    .Build();
                Velocity = targetVelocity;
                processStairs(targetVelocity);
                MoveAndSlide();
                applyTurning();
            }
        );

        _stateMachine.addStateProcessAction(
            State.Airborne,
            () =>
            {
                var direction = getMovementDirection();
                var targetVelocity = VelocityBuilder
                    .FromVelocity(Velocity)
                    .WithAcceleration(direction, _AirborneAcceleration, _curDelta)
                    .WithClampedHorizontalSpeed(_MaxHorizontalSpeed)
                    .WithGravity(_Gravity, _curDelta)
                    .Build();
                Velocity = targetVelocity;
                processStairs(targetVelocity);
                MoveAndSlide();
                applyTurning();
            }
        );

        _stateMachine.addStateTransition(State.Grounded, State.Airborne, () => !IsOnFloor());
        _stateMachine.addStateTransition(State.Airborne, State.Grounded, IsOnFloor);
    }

    public override void _Process(double delta)
    {
        _curDelta = (float)delta;
        _stateMachine.ProcessState();
    }

    private Vector3 getMovementDirection()
    {
        return Basis
            * new Vector3(
                Locator<InputManager>.Get().GetHorizontalMovementInput(),
                0,
                Locator<InputManager>.Get().GetVerticalMovementInput()
            ).Normalized();
    }

    private void applyTurning()
    {
        var mouseMotion = Locator<InputManager>.Get().GetAndResetMouseMotion();
        RotateY(Mathf.DegToRad(-mouseMotion.X * _MouseSensitivity));
        _pitch += Mathf.DegToRad(-mouseMotion.Y * _MouseSensitivity);
        _pitch = Mathf.Clamp(_pitch, Mathf.DegToRad(-90.0f), Mathf.DegToRad(90.0f));
        _camera.Rotation = new Vector3(_pitch, 0, 0);
    }

    private void processStairs(Vector3 targetVelocity)
    {
        var direction = new Vector3(targetVelocity.X, 0, targetVelocity.Y).Normalized();
        if (IsOnWall() && !direction.IsZeroApprox())
        {
            var stairDistanceOpt = probeStairDistance(direction);
            if (stairDistanceOpt.HasValue)
            {
                Translate(new Vector3(0, stairDistanceOpt.Value, 0));
            }
        }
    }

    private Nullable<float> probeStairDistance(Vector3 direction)
    {
        var origin =
            _StairProbe.GlobalPosition
            + (_StairProbeDistance * direction)
            + (_MaxStairHeight * Vector3.Up);
        var hit = new RaycastBuilder(_StairProbe)
            .FromPosition(origin)
            .WithDirectionAndMagnitude(Vector3.Down, _MaxStairHeight)
            .WithIgnoredObject(this)
            .Cast();
        return hit == null ? null : _MaxStairHeight - origin.DistanceTo(hit.Position);
    }

    private void QuickMoveAndSlide(Vector3 velocity)
    {
        var oldVelocity = Velocity;
        Velocity = velocity;
        MoveAndSlide();
        Velocity = oldVelocity;
    }

    public void PickUp(string message)
    {
        Locator<MessageManager>.Get().AddMessage(message);
    }

    public Player CreatePlayer()
    {
        return new Player(this, GetViewport().GetCamera3D());
    }
}
