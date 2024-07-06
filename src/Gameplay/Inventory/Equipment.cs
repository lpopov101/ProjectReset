using System;
using System.Collections.Generic;
using Godot;

public class Equipment
{
    public enum EquipSlot
    {
        Weapon,
        Armor,
        Test
    }

    public delegate void EquipEventHandler(EquippableInventoryItem inventoryItem, EquipSlot slot);

    public event EquipEventHandler EquipmentChanged;

    private Dictionary<EquipSlot, EquippableInventoryItem> _equipmentDict;

    private PriorityQueue<EquipSlot, ulong> _lastEquippedSlotQueue;

    public Equipment()
    {
        _equipmentDict = new Dictionary<EquipSlot, EquippableInventoryItem>();
        _lastEquippedSlotQueue = new PriorityQueue<EquipSlot, ulong>();
    }

    public void EquipItem(EquippableInventoryItem item)
    {
        var maybeBestEquipSlot = GetBestEquipSlot(item.GetCompatibleSlots());
        if (maybeBestEquipSlot.HasValue)
        {
            _equipmentDict[maybeBestEquipSlot.Value] = item;
            _lastEquippedSlotQueue.Enqueue(maybeBestEquipSlot.Value);
            item.SetEquipped(true);
        }
    }

    public void UnequipSlot(EquipSlot equipSlot)
    {
        if (_equipmentDict.ContainsKey(equipSlot))
        {
            var equippedItem = _equipmentDict[equipSlot];
            _equipmentDict.Remove(equipSlot);
            equippedItem.SetEquipped(false);
            EquipmentChanged.Invoke(equippedItem, equipSlot);
        }
    }

    public void UnequipItemIfEquipped(EquippableInventoryItem item)
    {
        if (GetEquipSlot(item) is EquipSlot slot)
        {
            UnequipSlot(slot);
        }
    }

    public Nullable<EquipSlot> GetEquipSlot(EquippableInventoryItem item)
    {
        foreach (var slot in item.GetCompatibleSlots())
        {
            if (
                _equipmentDict.ContainsKey(slot)
                && _equipmentDict[slot].GetName() == item.GetName()
            )
            {
                return slot;
            }
        }
        return null;
    }

    public EquippableInventoryItem GetItem(EquipSlot slot)
    {
        if (_equipmentDict.ContainsKey(slot))
        {
            return _equipmentDict[slot];
        }
        return null;
    }

    private Nullable<EquipSlot> GetBestEquipSlot(EquipSlot[] compatibleSlots)
    {
        if (compatibleSlots.Length == 0)
        {
            return null;
        }
        foreach (var compatibleSlot in compatibleSlots)
        {
            if (!_equipmentDict.ContainsKey(compatibleSlot))
            {
                return compatibleSlot;
            }
        }

        return null;
    }
}
