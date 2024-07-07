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

    public virtual string GetUseActionName()
    {
        return "Use";
    }

    public PackedScene GetDisplayModelTemplate()
    {
        return _DisplayModelTemplate;
    }

    public abstract void Use(Player player);
}
