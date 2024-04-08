using System;
using Godot;

public abstract partial class Hitscan : Node3D, ISpawnable
{
    [Export]
    private Vector3 _Direction = Vector3.Forward;

    [Export]
    private float _MaxDistance = 1000F;
    public Pool OriginPool { get; set; }

    public void OnDespawn() { }

    public void OnSpawn()
    {
        OnCast();
        var castPos = GlobalPosition;
        var hit = new RaycastBuilder(this)
            .FromPosition(castPos)
            .WithDirectionAndMagnitude(_Direction, _MaxDistance)
            .Cast();
        if (hit != null)
        {
            OnHit(hit.Collider, hit.Position, hit.Normal);
        }
        ((ISpawnable)this).Despawn();
    }

    protected abstract void OnCast();
    protected abstract void OnHit(GodotObject collider, Vector3 position, Vector3 normal);
}
