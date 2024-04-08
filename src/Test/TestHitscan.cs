using System;
using Godot;

public partial class TestHitscan : Hitscan
{
    [Export]
    private float _Damage = 10.0F;

    [Export]
    private PackedScene _CollisionEffect;

    protected override void OnCast() { }

    protected override void OnHit(GodotObject collider, Vector3 position, Vector3 normal)
    {
        if (collider != null && collider is IDamageable damageable)
        {
            damageable.Damage(_Damage);
        }
        var particle = GameManager.GetPool(_CollisionEffect).Spawn3D<ParticleEffect>();
        particle.MoveToPosAndNormal(position, normal);
    }
}
