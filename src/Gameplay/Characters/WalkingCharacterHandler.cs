using System;
using System.Runtime.InteropServices;
using System.Text;
using Godot;

[GlobalClass]
public partial class WalkingCharacterHandler : Node
{
    enum State
    {
        Grounded,
        Airborne,
    }

    [Export]
    private CharacterBody3D _CharacterBody = null;

    [Export]
    private WalkingCharacterSettings _WalkingCharacterSettings = null;

    [Export]
    private Node3D _StairProbe = null;

    private StateMachine<State> _stateMachine;

    private Vector2 _movementInput = Vector2.Zero;
    private float _rotationInput = 0F;
    private bool _JumpInput = false;

    public override void _Ready()
    {
        _stateMachine = new StateMachine<State>(State.Airborne);

        _stateMachine.addStateProcessAction(State.Grounded, applyGroundedMovement);
        _stateMachine.addStateProcessAction(State.Airborne, applyAirborneMovement);

        _stateMachine.addStateTransition(
            State.Grounded,
            State.Airborne,
            () => !_CharacterBody.IsOnFloor()
        );
        _stateMachine.addStateTransition(State.Airborne, State.Grounded, _CharacterBody.IsOnFloor);
    }

    public override void _Process(double delta)
    {
        if (_CharacterBody != null && _WalkingCharacterSettings != null)
        {
            _stateMachine.ProcessState(delta);
        }
    }

    public void Init(
        CharacterBody3D characterBody,
        WalkingCharacterSettings walkingCharacterSettings,
        Node3D stairProbe
    )
    {
        _CharacterBody = characterBody;
        _WalkingCharacterSettings = walkingCharacterSettings;
        _StairProbe = stairProbe;
        _CharacterBody.SetCollisionLayerValue(PlayerManager.PLAYER_COLLISION_LAYER, true);
        _CharacterBody.FloorSnapLength = _WalkingCharacterSettings._MaxStairHeight + 0.1F;
    }

    public void SetMovementInput(Vector2 input)
    {
        _movementInput = input;
    }

    public void SetRotationInput(float input)
    {
        _rotationInput = input;
    }

    public void SetJumpInput(bool input)
    {
        _JumpInput = input;
    }

    private void applyGroundedMovement(double delta)
    {
        var direction = getMovementDirection();
        var jumpForce = _JumpInput ? _WalkingCharacterSettings._JumpForce : 0F;
        var targetVelocity = VelocityBuilder
            .FromVelocity(_CharacterBody.Velocity)
            .WithGroundedMovement(
                direction,
                _WalkingCharacterSettings._GroundedMovementSpeed,
                _WalkingCharacterSettings._GroundedFriction,
                (float)delta
            )
            .WithClampedXZSpeed(_WalkingCharacterSettings._MaxXZSpeed)
            .WithJumping(jumpForce)
            .Build();
        applyTargetVelocity(targetVelocity, delta);
    }

    private void applyAirborneMovement(double delta)
    {
        var direction = getMovementDirection();
        var targetVelocity = VelocityBuilder
            .FromVelocity(_CharacterBody.Velocity)
            .WithAcceleration(
                direction,
                _WalkingCharacterSettings._AirborneAcceleration,
                (float)delta
            )
            .WithClampedXZSpeed(_WalkingCharacterSettings._MaxXZSpeed)
            .WithGravity(_WalkingCharacterSettings._Gravity, (float)delta)
            .Build();
        applyTargetVelocity(targetVelocity, delta);
    }

    private void applyTargetVelocity(Vector3 targetVelocity, double delta)
    {
        _CharacterBody.Velocity = targetVelocity;
        processStairs(targetVelocity, delta);
        _CharacterBody.MoveAndSlide();
        applyTurning(delta);
    }

    private Vector3 getMovementDirection()
    {
        return _CharacterBody.Basis
            * new Vector3(_movementInput.X, 0, _movementInput.Y).Normalized();
    }

    private void applyTurning(double delta)
    {
        _CharacterBody.RotateY(Mathf.DegToRad(_rotationInput * (float)delta));
    }

    private void processStairs(Vector3 targetVelocity, double delta)
    {
        var direction = new Vector3(targetVelocity.X, 0, targetVelocity.Z).Normalized();
        if (_CharacterBody.IsOnWall() && !direction.IsZeroApprox())
        {
            var stairDistanceOpt = probeStairDistance(direction);
            if (stairDistanceOpt.HasValue)
            {
                _CharacterBody.Translate(
                    new Vector3(0, Mathf.Lerp(0, stairDistanceOpt.Value, (float)delta * 30F), 0)
                );
            }
        }
    }

    private Nullable<float> probeStairDistance(Vector3 direction)
    {
        if (_StairProbe == null)
        {
            return null;
        }
        var origin =
            _StairProbe.GlobalPosition
            + _WalkingCharacterSettings._StairProbeDistance * direction
            + (_WalkingCharacterSettings._MaxStairHeight * Vector3.Up);
        var hit = new RaycastBuilder(_StairProbe)
            .FromPosition(origin)
            .WithDirectionAndMagnitude(Vector3.Down, _WalkingCharacterSettings._MaxStairHeight)
            .WithIgnoredObject(_CharacterBody)
            .Cast();
        if (hit != null)
        {
            return _WalkingCharacterSettings._MaxStairHeight - origin.DistanceTo(hit.Position);
        }
        return null;
    }
}
