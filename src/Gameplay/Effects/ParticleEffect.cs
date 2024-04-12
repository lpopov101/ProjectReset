using System;
using Godot;

public partial class ParticleEffect : GpuParticles3D, ISpawnable
{
    [Export]
    public AudioStream _SoundEffect;
    public Pool OriginPool { get; set; }

    public override void _Ready()
    {
        base._Ready();
        OneShot = true;
        Finished += () =>
        {
            ((ISpawnable)this).Despawn();
        };
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
    }

    public void OnDespawn() { }

    public void OnSpawn()
    {
        Restart();
        Emitting = true;
        if (_SoundEffect != null)
        {
            Locator<SoundManager>
                .Get()
                .Spawn3DAudio(_SoundEffect, GlobalPosition)
                .WithPitchVariation(0.2F)
                .Play();
        }
    }

    public void MoveToPosAndNormal(Vector3 position, Vector3 normal)
    {
        var lookAt = -(position + (normal * 100));
        var transform = new Transform3D(new Basis(), position);
        transform.Origin = position;
        GlobalTransform = LinAlgUtil.QuickLookAt(transform, lookAt);
    }
}
