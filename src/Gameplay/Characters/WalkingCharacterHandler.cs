using System;
using System.Runtime.InteropServices;
using System.Text;
using Godot;

public class WalkingCharacterHandler

{
    private CharacterBody3D _characterBody;

    public WalkingCharacterHandler(CharacterBody3D characterBody)
    {
        _characterBody = characterBody;
    }

    private void applyGroundedMovement(Vector3 direction, double delta, float jumpForce, float speed, float friction, float maxXZSpeed)
    {
        var appliedJumpForce = Locator<InputManager>.Get().GetJumpInput() ? jumpForce : 0F;
        var targetVelocity = VelocityBuilder
            .FromVelocity(_characterBody.Velocity)
            .WithGroundedMovement(
                direction,
                speed,
                friction,
                (float)delta
            )
            .WithClampedXZSpeed(maxXZSpeed)
            .WithJumping(appliedJumpForce)
            .Build();
        applyTargetVelocity(targetVelocity, delta);
    }

    private void applyAirborneMovement(Vector3 direction, double delta, float acceleration, float maxXZSpeed, float gravity)
    {
        var targetVelocity = VelocityBuilder
            .FromVelocity(_characterBody.Velocity)
            .WithAcceleration(direction, acceleration, (float)delta)
            .WithClampedXZSpeed(maxXZSpeed)
            .WithGravity(gravity, (float)delta)
            .Build();
        applyTargetVelocity(targetVelocity, delta);
    }

    private void applyTargetVelocity(Vector3 targetVelocity, double delta)
    {
        _characterBody.Velocity = targetVelocity;
        processStairs(targetVelocity, delta);
        _characterBody.MoveAndSlide();
    }

    private void processStairs(Vector3 targetVelocity, double delta)
    {
        var direction = new Vector3(targetVelocity.X, 0, targetVelocity.Z).Normalized();
        if (_characterBody.IsOnWall() && !direction.IsZeroApprox())
        {
            var stairDistanceOpt = probeStairDistance(direction);
            if (stairDistanceOpt.HasValue)
            {
                _characterBody.Translate(new Vector3(0, Mathf.Lerp(0, stairDistanceOpt.Value, (float)delta * 30F), 0));
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
}
