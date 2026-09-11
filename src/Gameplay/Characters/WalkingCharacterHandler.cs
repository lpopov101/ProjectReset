using System;
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
    private bool _stuckToGround = false;

    public override void _Ready()
    {
        _stateMachine = new StateMachine<State>(State.Airborne);

        _stateMachine.addStateProcessAction(State.Grounded, applyGroundedMovement);
        _stateMachine.addStateProcessAction(State.Airborne, applyAirborneMovement);

        _stateMachine.addStateTransition(
            State.Grounded,
            State.Airborne,
            () =>
            {
                return !_CharacterBody.IsOnFloor() && !_stuckToGround;
            }
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
        var builder = VelocityBuilder
            .FromVelocity(_CharacterBody.Velocity)
            .WithGroundedMovement(
                direction,
                _WalkingCharacterSettings._GroundedMovementSpeed,
                _WalkingCharacterSettings._GroundedFriction,
                (float)delta
            )
            .WithClampedXZSpeed(_WalkingCharacterSettings._MaxXZSpeed);
        if (_JumpInput)
        {
            builder = builder.WithJumping(_WalkingCharacterSettings._JumpForce);
        }
        applyStepClimb(direction, delta);
        applyTargetVelocity(builder.Build(), delta);
        applyGroundStick();
    }

    private void applyAirborneMovement(double delta)
    {
        var direction = getMovementDirection();
        var builder = VelocityBuilder
            .FromVelocity(_CharacterBody.Velocity)
            .WithAcceleration(
                direction,
                _WalkingCharacterSettings._AirborneAcceleration,
                (float)delta
            )
            .WithClampedXZSpeed(_WalkingCharacterSettings._MaxXZSpeed)
            .WithGravity(_WalkingCharacterSettings._Gravity, (float)delta);
        applyStepClimb(direction, delta);
        applyTargetVelocity(builder.Build(), delta);
    }

    private void applyTargetVelocity(Vector3 targetVelocity, double delta)
    {
        _CharacterBody.Velocity = targetVelocity;
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

    private Vector3 getFootPosition()
    {
        return _StairProbe != null ? _StairProbe.GlobalPosition : _CharacterBody.GlobalPosition;
    }

    private float getDistanceToGround(float maxDistance)
    {
        var fromPosition = getFootPosition();
        var hit = new RaycastBuilder(_CharacterBody)
            .FromPosition(fromPosition)
            .WithDirectionAndMagnitude(Vector3.Down, maxDistance)
            .WithIgnoredObject(_CharacterBody)
            .WithHitBackFaces(false)
            .Cast();
        if (hit != null && isWalkableSurface(hit))
        {
            return fromPosition.DistanceTo(hit.Position);
        }
        return float.MaxValue;
    }

    private void applyGroundStick()
    {
        _stuckToGround = false;
        if (_CharacterBody.IsOnFloor() || _CharacterBody.Velocity.Y > 0F)
        {
            return;
        }
        var maxDrop = _WalkingCharacterSettings._MaxStairHeight;
        var distance = getDistanceToGround(maxDrop + _WalkingCharacterSettings._StepClearance);
        if (distance > maxDrop)
        {
            return;
        }
        _CharacterBody.MoveAndCollide(Vector3.Down * distance);
        _stuckToGround = true;
    }

    private void applyStepClimb(Vector3 direction, double delta)
    {
        if (!_CharacterBody.IsOnWall() || direction.IsZeroApprox())
        {
            return;
        }
        var stepHeightOpt = probeStepHeight(direction);
        if (!stepHeightOpt.HasValue)
        {
            return;
        }
        var remaining = stepHeightOpt.Value + _WalkingCharacterSettings._StepClearance;
        var rise = Mathf.Min(
            remaining,
            _WalkingCharacterSettings._StepClimbSpeed * (float)delta
        );
        if (rise <= 0F)
        {
            return;
        }
        _CharacterBody.MoveAndCollide(Vector3.Up * rise);
    }

    private Nullable<float> probeStepHeight(Vector3 direction)
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
            .WithHitBackFaces(false)
            .Cast();
        if (hit == null || !isWalkableSurface(hit))
        {
            return null;
        }
        return _WalkingCharacterSettings._MaxStairHeight - origin.DistanceTo(hit.Position);
    }

    private bool isWalkableSurface(RaycastHit hit)
    {
        return hit.Normal.Dot(_CharacterBody.UpDirection)
            >= Mathf.Cos(_CharacterBody.FloorMaxAngle);
    }
}
