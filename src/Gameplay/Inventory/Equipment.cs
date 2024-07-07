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

    private Dictionary<EquipSlot, ulong> _lastEquippedTimeDict;

    public Equipment()
    {
        _equipmentDict = new Dictionary<EquipSlot, EquippableInventoryItem>();
        _lastEquippedTimeDict = new Dictionary<EquipSlot, ulong>();
    }

    public bool EquipItem(EquippableInventoryItem item)
    {
        if (GetBestEquipSlot(item.GetCompatibleSlots()) is EquipSlot bestEquipSlot)
        {
            _equipmentDict[bestEquipSlot] = item;
            _lastEquippedTimeDict[bestEquipSlot] = Time.GetTicksMsec();
            item.SetEquipped(true);
            EquipmentChanged.Invoke(item, bestEquipSlot);
            return true;
        }
        return false;
    }

    public bool UnequipSlot(EquipSlot equipSlot)
    {
        if (_equipmentDict.ContainsKey(equipSlot))
        {
            var equippedItem = _equipmentDict[equipSlot];
            _equipmentDict.Remove(equipSlot);
            equippedItem.SetEquipped(false);
            EquipmentChanged.Invoke(equippedItem, equipSlot);
            return true;
        }
        return false;
    }

    public bool UnequipItem(EquippableInventoryItem item)
    {
        if (GetEquipSlot(item) is EquipSlot slot)
        {
            return UnequipSlot(slot);
        }
        return false;
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
        Nullable<EquipSlot> mostStaleSlot = null;
        var mostStaleTime = ulong.MaxValue;
        foreach (var compatibleSlot in compatibleSlots)
        {
            if (!_equipmentDict.ContainsKey(compatibleSlot))
            {
                return compatibleSlot;
            }

            if (
                _lastEquippedTimeDict.ContainsKey(compatibleSlot)
                && _lastEquippedTimeDict[compatibleSlot] < mostStaleTime
            )
            {
                mostStaleTime = _lastEquippedTimeDict[compatibleSlot];
                mostStaleSlot = compatibleSlot;
            }
        }

        return mostStaleSlot;
    }
}
