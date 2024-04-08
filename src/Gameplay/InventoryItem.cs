using System;
using Godot;

public abstract partial class InventoryItem : Resource
{
    [Export]
    protected string _Name;

    [Export]
    protected string _Description;

    [Export]
    protected int _Weight;

    [Export]
    protected PackedScene _DisplayModelTemplate;

    protected Inventory _inventory;

    public InventoryItem()
        : this("unknown", "unknown", 1, null, null) { }

    public InventoryItem(
        string name,
        string description,
        int weight,
        PackedScene displayModelTemplate,
        Inventory inventory
    )
    {
        _Name = name;
        _Description = description;
        _Weight = weight;
        _DisplayModelTemplate = displayModelTemplate;
        _inventory = inventory;
    }

    public void SetInventory(Inventory inventory)
    {
        _inventory = inventory;
    }

    public string GetName()
    {
        return _Name;
    }

    public string GetDescription()
    {
        return _Description;
    }

    public int GetWeight()
    {
        return _Weight;
    }

    public PackedScene GetDisplayModelTemplate()
    {
        return _DisplayModelTemplate;
    }

    public abstract void Use(Player player);
}
