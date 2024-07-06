using System;
using Godot;

[GlobalClass]
public partial class TestEquippableInventoryItem : EquippableInventoryItem
{
    public TestEquippableInventoryItem()
        : base()
    {
        _compatibleSlots = new Equipment.EquipSlot[] { Equipment.EquipSlot.Test };
    }
}
