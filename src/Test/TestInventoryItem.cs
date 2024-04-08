using System;
using Godot;

[GlobalClass]
public partial class TestInventoryItem : InventoryItem
{
    public TestInventoryItem()
        : base() { }

    public override void Use(Player player)
    {
        GameManager.Player1().Heal(10);
        _inventory.RemoveItem(this);
    }
}
