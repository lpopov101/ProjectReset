using System;
using Godot;

public partial class TestNPC : NPC
{
    // private MeshInstance3D _mesh;
    // private StandardMaterial3D _material;

    public override void _Ready()
    {
        base._Ready();
        // _mesh = GetNode<MeshInstance3D>("MeshInstance3D");
        // _material = _mesh.GetSurfaceOverrideMaterial(0) as StandardMaterial3D;
        _Health = _MaxHealth;
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        var playerPosition = Locator<PlayerManager>
            .Get()
            .Player1()
            .GetCharacterBody()
            .GlobalPosition;
        // Transform = Transform.LookingAt(
        //     GlobalPosition + (GlobalPosition - playerPosition),
        //     Vector3.Up
        // );
        setTargetPosition(playerPosition);
        Velocity = getTargetVelocity();
        Locator<MessageManager>.Get().AddMessage("target velocity: " + getTargetVelocity());
        MoveAndSlide();
    }

    public override void Damage(
        float damage,
        IDamageable.DamageType damageType = IDamageable.DamageType.Regular
    )
    {
        _Health -= damage;
        // _material.AlbedoColor = new Color(1F, _Health / _MaxHealth, _Health / _MaxHealth);
        if (_Health <= 0)
        {
            ((ISpawnable)this).Despawn();
        }
    }

    public override void OnDespawn()
    {
        base.OnDespawn();
        // _material.AlbedoColor = new Color(1F, 1F, 1F);
    }
}
