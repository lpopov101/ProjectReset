using System;
using Godot;

public partial class TestProjectile : Projectile
{
    [Export]
    private float _Damage = 10.0F;

    [Export]
    private float _LaunchImpulseMangitude = 10F;

    [Export]
    private PackedScene _CollisionEffect;

    protected override void OnLaunch()
    {
        ApplyImpulse(Transform.Basis.Z * -_LaunchImpulseMangitude);
    }

    protected override void OnCollide(Node3D body, Vector3 contactPosition, Vector3 contactNormal)
    {
        if (body is IDamageable damageable)
        {
            damageable.Damage(_Damage);
        }
        var particle = GameManager.GetPool(_CollisionEffect).Spawn3D<ParticleEffect>();
        particle.MoveToPosAndNormal(contactPosition, contactNormal);
    }

    private static Vector3 getPerpendicularVector(Vector3 vector)
    {
        return vector.Cross(new Vector3(vector.Y, vector.X, 0));
    }
}
