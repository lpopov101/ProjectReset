using System;
using System.Text;
using Godot;

public abstract partial class NPC : CharacterBody3D, IDamageable, ISpawnable
{
    [Export]
    public float _MaxHealth = 100F;

    [Export]
    public float _Speed = 10F;

    public bool _IsSpawned { get; private set; } = false;

    private NavigationAgent3D _navAgent;

    public float _Health { get; set; }
    public Pool OriginPool { get; set; }

    public override void _Ready()
    {
        _Health = _MaxHealth;
        _navAgent = GetNode<NavigationAgent3D>("NavigationAgent3D");
    }

    protected Vector3 getTargetVelocity()
    {
        if (_navAgent.IsNavigationFinished())
        {
            return Vector3.Zero;
        }
        var nextPosition = _navAgent.GetNextPathPosition();
        return GlobalPosition.DirectionTo(nextPosition) * _Speed;
    }

    protected void setTargetPosition(Vector3 targetPosition)
    {
        _navAgent.TargetPosition = targetPosition;
    }

    public virtual Vector3 GetTargetPosition()
    {
        return _navAgent.TargetPosition;
    }

    public abstract void Damage(
        float damage,
        IDamageable.DamageType damageType = IDamageable.DamageType.Regular
    );

    public virtual void OnSpawn()
    {
        _IsSpawned = true;
    }

    public virtual void OnDespawn()
    {
        _IsSpawned = false;
    }
}
