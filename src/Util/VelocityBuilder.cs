using System;
using Godot;

class VelocityBuilder
{
    private Vector3 _velocity { get; set; }

    public VelocityBuilder()
    {
        _velocity = Vector3.Zero;
    }

    public static VelocityBuilder FromVelocity(Vector3 velocity)
    {
        VelocityBuilder result = new VelocityBuilder();
        result._velocity = velocity;
        return result;
    }

    public VelocityBuilder WithGroundedMovement(
        Vector3 direction,
        float speed,
        float friction,
        float delta
    )
    {
        var curSpeedVector = new Vector2(_velocity.X, _velocity.Z);
        var targetSpeedVector = new Vector2(direction.X, direction.Z).Normalized() * speed;
        var speedVector = applyXZVelocityWithFriction(
            curSpeedVector,
            targetSpeedVector,
            friction,
            delta
        );
        return FromVelocity(new Vector3(speedVector.X, _velocity.Y, speedVector.Y));
    }

    public VelocityBuilder WithAcceleration(Vector3 direction, float acceleration, float delta)
    {
        return FromVelocity(_velocity + (direction * acceleration * delta));
    }

    public VelocityBuilder WithGravity(float gravity, float delta)
    {
        return FromVelocity(_velocity - (Vector3.Down * gravity * delta));
    }

    public VelocityBuilder WithJumping(float jumpForce)
    {
        return FromVelocity(new Vector3(_velocity.X, jumpForce, _velocity.Z));
    }

    public VelocityBuilder WithClampedXZSpeed(float maxHorizontalSpeed)
    {
        var horizontalVelocity = new Vector2(_velocity.X, _velocity.Z);
        if (horizontalVelocity.Length() <= maxHorizontalSpeed)
        {
            return this;
        }
        horizontalVelocity = horizontalVelocity.Normalized() * maxHorizontalSpeed;
        return FromVelocity(new Vector3(horizontalVelocity.X, _velocity.Y, horizontalVelocity.Y));
    }

    public Vector3 Build()
    {
        return _velocity;
    }

    private Vector2 applyXZVelocityWithFriction(
        Vector2 curVelocity,
        Vector2 appliedVelocity,
        float friction,
        float delta
    )
    {
        var adjustedVelocity = new Vector2(
            Mathf.Lerp(curVelocity.X, appliedVelocity.X, friction * delta),
            Mathf.Lerp(curVelocity.Y, appliedVelocity.Y, friction * delta)
        );
        if (adjustedVelocity.Length() <= Mathf.Epsilon)
        {
            adjustedVelocity = Vector2.Zero;
        }
        return adjustedVelocity;
    }
}
