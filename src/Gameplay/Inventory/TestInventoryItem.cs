using System;
using Godot;

[GlobalClass]
public partial class TestInventoryItem : InventoryItem
{
    public TestInventoryItem()
        : base() { }

    public override void Use(Player player)
    {
        player.Heal(10);
        player.GetInventory().RemoveItem(this);
    }
}
