using System;
using Godot;

public abstract partial class Projectile : RigidBody3D, ISpawnable
{
    [Export]
    private float _MaxLifespanSecs = 5.0F;
    private PhysicsDirectBodyState3D _state;
    private Timer _cooldownTimer;
    public Pool OriginPool { get; set; }

    public void OnDespawn() { }

    public void OnSpawn()
    {
        Sleeping = true;
        OnLaunch();
        _cooldownTimer.Start(_MaxLifespanSecs);
    }

    public override void _Ready()
    {
        base._Ready();
        if (_MaxLifespanSecs > 0)
        {
            _cooldownTimer = new Timer { Autostart = false, OneShot = true };
            _cooldownTimer.Timeout += () =>
            {
                ((ISpawnable)this).Despawn();
            };
            AddChild(_cooldownTimer);
        }

        ContactMonitor = true;
        MaxContactsReported = Mathf.Max(1, MaxContactsReported);
        BodyEntered += (body) =>
        {
            if (body is Node3D node3D)
            {
                OnCollide(
                    node3D,
                    _state.GetContactColliderPosition(0),
                    _state.GetContactLocalNormal(0)
                );
                ((ISpawnable)this).Despawn();
            }
        };
    }

    public override void _IntegrateForces(PhysicsDirectBodyState3D state)
    {
        base._IntegrateForces(state);
        _state = state;
    }

    protected abstract void OnLaunch();

    protected abstract void OnCollide(Node3D body, Vector3 contactPosition, Vector3 contactNormal);
}
