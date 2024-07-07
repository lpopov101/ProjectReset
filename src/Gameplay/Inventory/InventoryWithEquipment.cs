using System;
using System.Collections.Generic;
using Godot;

public class InventoryWithEquipment : Inventory
{
    private Equipment _equipment;

    public InventoryWithEquipment(int capacity = 100)
        : base(capacity)
    {
        _equipment = new Equipment();
        _equipment.EquipmentChanged += (item, slot) =>
        {
            InvokeItemChanged(item);
        };
    }

    public override void RemoveItem(InventoryItem item, int quantity = 1)
    {
        if (
            item is EquippableInventoryItem equippableItem
            && GetItemWithQuantity(item).Quantity - quantity <= 0
        )
        {
            _equipment.UnequipItemIfEquipped(equippableItem);
        }
        base.RemoveItem(item, quantity);
    }

    public Equipment GetEquipment()
    {
        return _equipment;
    }
}
