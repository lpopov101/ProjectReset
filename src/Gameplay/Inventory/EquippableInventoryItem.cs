using System;
using System.Collections.Generic;
using Godot;

public abstract partial class EquippableInventoryItem : InventoryItem
{
    protected Equipment.EquipSlot[] _compatibleSlots;
    protected bool _equipped = false;

    public EquippableInventoryItem()
        : base() { }

    public override void Use(Player player)
    {
        if (_equipped)
        {
            player.GetInventory().GetEquipment().UnequipItemIfEquipped(this);
        }
        else
        {
            player.GetInventory().GetEquipment().EquipItem(this);
        }
    }

    public Equipment.EquipSlot[] GetCompatibleSlots()
    {
        return _compatibleSlots;
    }

    public override string GetUseActionName()
    {
        if (_equipped)
        {
            return "Unequip";
        }
        else
        {
            return "Equip";
        }
    }

    public void SetEquipped(bool equipped)
    {
        _equipped = equipped;
    }

    public bool GetEquipped()
    {
        return _equipped;
    }
}
