using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class InventoryScrollContainer : ScrollContainer
{
    public delegate void EntrySelectedEventHandler(InventoryItem inventoryItem);

    public event EntrySelectedEventHandler EntrySelected;

    [Export]
    private PackedScene _EntryTemplate;

    private Container _childContainer;

    private Dictionary<string, InventoryEntry> _entryDict;

    private InventoryItem _selectedItem;

    private Pool _entryPool;

    public override void _Ready()
    {
        base._Ready();
        _childContainer = GetChild<Container>(0);
        _entryPool = Locator<SpawnManager>.Get().GetPool(_EntryTemplate);
        _entryDict = new Dictionary<string, InventoryEntry>();
    }

    public InventoryEntry GetOrAddEntry(InventoryItem item)
    {
        if (_entryDict.ContainsKey(item.GetName()))
        {
            return _entryDict[item.GetName()];
        }
        var entry = _entryPool.SpawnAsChild<InventoryEntry>(_childContainer);
        entry.Selected += (item) =>
        {
            _selectedItem = item;
            foreach (var keyVal in _entryDict)
            {
                var name = keyVal.Key;
                var entry = keyVal.Value;
                if (name != _selectedItem.GetName())
                {
                    entry.MakeDeselected();
                }
            }
            EntrySelected.Invoke(_selectedItem);
        };
        entry.SetInventoryItem(item);
        entry.SetQuantity(0);
        _entryDict[item.GetName()] = entry;
        AutoSelect();
        return entry;
    }

    public void RemoveEntry(InventoryItem item)
    {
        if (_entryDict.ContainsKey(item.GetName()))
        {
            if (_selectedItem.GetName() == item.GetName())
            {
                _selectedItem = null;
            }
            var entry = _entryDict[item.GetName()];
            _entryDict.Remove(item.GetName());
            ((ISpawnable)entry).Despawn();
            AutoSelect();
        }
    }

    public void AutoSelect()
    {
        if (_entryDict.Count > 0 && _selectedItem == null)
        {
            _entryDict.Values.First().MakeSelected();
        }
    }

    public InventoryItem GetSelectedItem()
    {
        return _selectedItem;
    }
}
