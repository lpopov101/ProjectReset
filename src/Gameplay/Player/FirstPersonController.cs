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
    private float _GroundedMovementSpeed = 7.5f;

    [Export]
    private float _GroundedFriction = 15.0F;

    [Export]
    private float _MaxXZSpeed = 10.0f;

    [Export]
    private float _AirborneAcceleration = 100.0F;

    [Export]
    private float _Gravity = -9.8f;

    [Export]
    private float _JumpForce = 4.5f;

    [Export]
    private float _MaxStairHeight = 1f;

    [Export]
    private Node3D _StairProbe;

    [Export]
    private float _StairProbeDistance = 0.75F;

    private StateMachine<State> _stateMachine;
    private Camera3D _camera;
    private float _pitch = 0.0f;

    public override void _Ready()
    {
        SetCollisionLayerValue(PlayerManager.PLAYER_COLLISION_LAYER, true);
        _stateMachine = new StateMachine<State>(State.Airborne);
        _camera = GetViewport().GetCamera3D();
        FloorSnapLength = _MaxStairHeight + 0.1F;

        _stateMachine.addStateProcessAction(State.Grounded, applyGroundedMovement);
        _stateMachine.addStateProcessAction(State.Airborne, applyAirborneMovement);

        _stateMachine.addStateTransition(State.Grounded, State.Airborne, () => !IsOnFloor());
        _stateMachine.addStateTransition(State.Airborne, State.Grounded, IsOnFloor);
    }

    public override void _Process(double delta)
    {
        _stateMachine.ProcessState(delta);
    }

    private void applyGroundedMovement(double delta)
    {
        var direction = getMovementDirection();
        var jumpForce = Locator<InputManager>.Get().GetJumpInput() ? _JumpForce : 0F;
        var targetVelocity = VelocityBuilder
            .FromVelocity(Velocity)
            .WithGroundedMovement(
                direction,
                _GroundedMovementSpeed,
                _GroundedFriction,
                (float)delta
            )
            .WithClampedXZSpeed(_MaxXZSpeed)
            .WithJumping(jumpForce)
            .Build();
        applyTargetVelocity(targetVelocity, delta);
    }

    private void applyAirborneMovement(double delta)
    {
        var direction = getMovementDirection();
        var targetVelocity = VelocityBuilder
            .FromVelocity(Velocity)
            .WithAcceleration(direction, _AirborneAcceleration, (float)delta)
            .WithClampedXZSpeed(_MaxXZSpeed)
            .WithGravity(_Gravity, (float)delta)
            .Build();
        applyTargetVelocity(targetVelocity, delta);
    }

    private void applyTargetVelocity(Vector3 targetVelocity, double delta)
    {
        Velocity = targetVelocity;
        processStairs(targetVelocity, delta);
        MoveAndSlide();
        applyTurning();
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

    private void processStairs(Vector3 targetVelocity, double delta)
    {
        var direction = new Vector3(targetVelocity.X, 0, targetVelocity.Z).Normalized();
        if (IsOnWall() && !direction.IsZeroApprox())
        {
            var stairDistanceOpt = probeStairDistance(direction);
            if (stairDistanceOpt.HasValue)
            {
                Translate(new Vector3(0, Mathf.Lerp(0, stairDistanceOpt.Value, (float)delta * 30F), 0));
            }
        }
    }

    private Nullable<float> probeStairDistance(Vector3 direction)
    {
        var origin =
            _StairProbe.GlobalPosition
            + _StairProbeDistance * direction
            + (_MaxStairHeight * Vector3.Up);
        var hit = new RaycastBuilder(_StairProbe)
            .FromPosition(origin)
            .WithDirectionAndMagnitude(Vector3.Down, _MaxStairHeight)
            .WithIgnoredObject(this)
            .Cast();
        if (hit != null)
        {
            return _MaxStairHeight - origin.DistanceTo(hit.Position);
        }
        return null;
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
