using System;
using System.Collections.Generic;
using Godot;

public class Player : IDamageable
{
    private CharacterBody3D _characterBody;
    private Camera3D _camera;

    private InventoryWithEquipment _inventory;

    public float _Health { get; set; }

    public Player(CharacterBody3D characterBody, Camera3D camera, int inventoryMaxCapacity = 100)
    {
        _characterBody = characterBody;
        _camera = camera;
        _inventory = new InventoryWithEquipment(inventoryMaxCapacity);
    }

    public CharacterBody3D GetCharacterBody()
    {
        return _characterBody;
    }

    public Camera3D GetCamera()
    {
        return _camera;
    }

    public InventoryWithEquipment GetInventory()
    {
        return _inventory;
    }

    public void Damage(
        float damage,
        IDamageable.DamageType damageType = IDamageable.DamageType.Regular
    )
    {
        _Health -= damage;
    }

    public void Heal(float healAmount)
    {
        Damage(-healAmount);
    }
}
