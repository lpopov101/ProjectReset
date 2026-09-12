using Godot;

// Minimal Projectile for DespawnTestScene. Records collisions instead of
// spawning effects so the harness carries no scene dependencies of its own.
public partial class DespawnTestProjectile : Projectile
{
    [Export]
    private float _LaunchImpulse = 20F;

    public int CollideCount { get; private set; } = 0;

    protected override void OnLaunch()
    {
        ApplyImpulse(Transform.Basis.Z * -_LaunchImpulse);
    }

    protected override void OnCollide(Node3D body, Vector3 contactPosition, Vector3 contactNormal)
    {
        CollideCount++;
    }
}
