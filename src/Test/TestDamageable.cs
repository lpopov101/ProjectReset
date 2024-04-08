using System;
using Godot;

public partial class TestDamageable : StaticBody3D, IDamageable
{
    [Export]
    private MeshInstance3D _Mesh;

    [Export]
    private float _MaxHealth = 50F;
    private StandardMaterial3D _material;
    public float _Health { get; set; }

    public override void _Ready()
    {
        base._Ready();
        _material = _Mesh.GetSurfaceOverrideMaterial(0) as StandardMaterial3D;
        _Health = _MaxHealth;
    }

    public void Damage(
        float damage,
        IDamageable.DamageType damageType = IDamageable.DamageType.Regular
    )
    {
        _Health -= damage;
        _material.AlbedoColor = new Color(1F, _Health / _MaxHealth, _Health / _MaxHealth);
    }
}
