using System;
using Godot;

[GlobalClass]
public partial class WalkingCharacterSettings : Resource
{
    [Export]
    public float _GroundedMovementSpeed { get; private set; } = 7.5f;

    [Export]
    public float _GroundedFriction { get; private set; } = 15.0F;

    [Export]
    public float _MaxXZSpeed { get; private set; } = 10.0f;

    [Export]
    public float _AirborneAcceleration { get; private set; } = 100.0F;

    [Export]
    public float _Gravity { get; private set; } = -9.8f;

    [Export]
    public float _JumpForce { get; private set; } = 4.5f;

    [Export]
    public float _MaxStairHeight { get; private set; } = 1f;

    [Export]
    public float _StairProbeDistance { get; private set; } = 0.75F;
}
