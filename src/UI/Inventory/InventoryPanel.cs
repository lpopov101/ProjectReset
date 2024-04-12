using System;
using Godot;

public partial class InventoryPanel : Control
{
    [Export]
    private RichTextLabel _WeightLabel;

    [Export]
    private InventoryScrollContainer _InventoryScrollContainer;

    [Export]
    private InventoryItemView _InventoryItemView;

    private Inventory _inventory;

    public override void _Ready()
    {
        base._Ready();
        _inventory = Locator<PlayerManager>.Get().Player1().GetInventory();
        updateWeightLabel();
        _inventory.InventoryItemChanged += (item) =>
        {
            var itemWithQuantity = _inventory.GetItemWithQuantity(item);
            if (itemWithQuantity == null)
            {
                _InventoryScrollContainer.RemoveEntry(item);
            }
            else
            {
                var entry = _InventoryScrollContainer.GetOrAddEntry(item);
                entry.SetQuantity(itemWithQuantity.Quantity);
            }
            updateWeightLabel();
        };
        _InventoryScrollContainer.EntrySelected += (item) =>
        {
            _InventoryItemView.SetItem(item);
        };
        _InventoryItemView.Use += (item) =>
        {
            _inventory.GetItemWithQuantity(item).Item.Use(Locator<PlayerManager>.Get().Player1());
            clearItemViewIfNoItem();
        };
        _InventoryItemView.Discard += (item) =>
        {
            _inventory.RemoveItem(item);
            clearItemViewIfNoItem();
        };
    }

    private void clearItemViewIfNoItem()
    {
        if (_InventoryScrollContainer.GetSelectedItem() == null)
        {
            _InventoryItemView.ClearItem();
        }
    }

    private void updateWeightLabel()
    {
        var weightCapacityTuple = _inventory.GetTotalWeightAndMaxCapacity();
        var totalWeight = weightCapacityTuple.Item1;
        var maxCapacity = weightCapacityTuple.Item2;
        _WeightLabel.Text = $"Total Weight: {totalWeight}/{maxCapacity}";
    }
}
