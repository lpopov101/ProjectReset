using System;
using System.Collections.Generic;
using Godot;

public class Player : IDamageable
{
    public enum EquipSlot
    {
        Weapon,
        Armor
    }

    public delegate void EquipEventHandler(InventoryItem inventoryItem);

    public event EquipEventHandler ItemEquipped;

    public event EquipEventHandler ItemUnequipped;

    private CharacterBody3D _characterBody;
    private Camera3D _camera;

    private Inventory _inventory;

    public float _Health { get; set; }

    public Player(CharacterBody3D characterBody, Camera3D camera, int inventoryMaxCapacity = 100)
    {
        _characterBody = characterBody;
        _camera = camera;
        _inventory = new Inventory(inventoryMaxCapacity);
    }

    public CharacterBody3D GetCharacterBody()
    {
        return _characterBody;
    }

    public Camera3D GetCamera()
    {
        return _camera;
    }

    public Inventory GetInventory()
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

    public void EquipItem(InventoryItem item)
    {
        if (_inventory.GetItemWithQuantity(item) == null)
        {
            return;
        }
        ItemEquipped.Invoke(item);
    }

    public void UnequipItem(InventoryItem item)
    {
        ItemUnequipped.Invoke(item);
    }
}
