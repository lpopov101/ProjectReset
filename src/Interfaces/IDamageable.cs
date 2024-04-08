using System;
using Godot;

public interface IDamageable
{
    enum DamageType
    {
        Regular,
    }

    public float _Health { get; set; }

    public void Damage(float damage, DamageType damageType = DamageType.Regular);
}
