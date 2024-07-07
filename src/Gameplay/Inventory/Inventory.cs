using System;
using System.Collections.Generic;
using Godot;

public class Inventory
{
    public class InventoryItemWithQuantity
    {
        public InventoryItem Item;
        public int Quantity;

        public InventoryItemWithQuantity(InventoryItem item, int quantity)
        {
            Item = item;
            Quantity = quantity;
        }
    }

    public delegate void InventoryEventHandler(InventoryItem inventoryItem);

    public event InventoryEventHandler InventoryItemChanged;

    private int _maxCapacity;
    private int _remainingCapcaity;
    private Dictionary<string, InventoryItemWithQuantity> _itemDict;

    public Inventory(int capacity = 100)
    {
        _maxCapacity = capacity;
        _remainingCapcaity = capacity;
        _itemDict = new Dictionary<string, InventoryItemWithQuantity>();
    }

    public bool ContainsItem(InventoryItem item)
    {
        return _itemDict.ContainsKey(item.GetName());
    }

    public bool TryAddItem(InventoryItem item, int quantity = 1)
    {
        if (item.GetWeight() * quantity > _remainingCapcaity)
        {
            return false;
        }
        var name = item.GetName();
        if (!_itemDict.ContainsKey(name))
        {
            _itemDict[name] = new InventoryItemWithQuantity(item, 0);
            _itemDict[name].Item.SetInventory(this);
        }
        _itemDict[name].Quantity += quantity;
        _remainingCapcaity -= item.GetWeight() * quantity;
        InvokeItemChanged(item);
        return true;
    }

    public virtual void RemoveItem(InventoryItem item, int quantity = 1)
    {
        if (_itemDict.ContainsKey(item.GetName()))
        {
            var itemWithQuantity = _itemDict[item.GetName()];
            itemWithQuantity.Quantity -= quantity;
            _remainingCapcaity += itemWithQuantity.Item.GetWeight() * quantity;
            if (itemWithQuantity.Quantity <= 0)
            {
                _itemDict.Remove(item.GetName());
            }
            InvokeItemChanged(itemWithQuantity.Item);
        }
    }

    protected void InvokeItemChanged(InventoryItem item)
    {
        InventoryItemChanged.Invoke(item);
    }

    public InventoryItemWithQuantity GetItemWithQuantity(string name)
    {
        return _itemDict.GetValueOrDefault(name, null);
    }

    public InventoryItemWithQuantity GetItemWithQuantity(InventoryItem item)
    {
        return GetItemWithQuantity(item.GetName());
    }

    public Tuple<int, int> GetTotalWeightAndMaxCapacity()
    {
        return new Tuple<int, int>(_maxCapacity - _remainingCapcaity, _maxCapacity);
    }
}
