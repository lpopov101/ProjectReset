using System;
using Godot;

public partial class InventoryEntry : Control, ISpawnable
{
    public delegate void SelectedEventHandler(InventoryItem inventoryItem);

    public event SelectedEventHandler Selected;

    [Export]
    private TextureRect _SelectedIndicator;

    [Export]
    private RichTextLabel _NameLabel;

    [Export]
    private RichTextLabel _WeightLabel;

    [Export]
    private BaseButton _SelectButton;

    private bool _selected = false;
    private Inventory.InventoryItemWithQuantity _inventoryItemWithQuantity;
    public Pool OriginPool { get; set; }

    public void OnDespawn()
    {
        _selected = false;
        setSelectedIndicatorAlpha(0F);
    }

    public void OnSpawn() { }

    public override void _Ready()
    {
        base._Ready();
        _SelectButton.ToggleMode = false;
        setSelectedIndicatorAlpha(0F);
        _SelectButton.Pressed += () =>
        {
            MakeSelected();
        };
        _SelectButton.MouseEntered += () =>
        {
            if (!_selected)
            {
                setSelectedIndicatorAlpha(0.5F);
            }
        };
        _SelectButton.MouseExited += () =>
        {
            if (!_selected)
            {
                setSelectedIndicatorAlpha(0F);
            }
        };
    }

    public void MakeSelected()
    {
        _selected = true;
        setSelectedIndicatorAlpha(1F);
        Selected.Invoke(_inventoryItemWithQuantity.Item);
    }

    public void MakeDeselected()
    {
        _selected = false;
        setSelectedIndicatorAlpha(0);
    }

    public void SetInventoryItem(InventoryItem item)
    {
        _inventoryItemWithQuantity = new Inventory.InventoryItemWithQuantity(item, 0);
        UpdateLabels();
    }

    public void SetQuantity(int quantity)
    {
        _inventoryItemWithQuantity.Quantity = quantity;
        UpdateLabels();
    }

    public InventoryItem GetInventoryItem()
    {
        return _inventoryItemWithQuantity.Item;
    }

    public int GetQuantity()
    {
        return _inventoryItemWithQuantity.Quantity;
    }

    private void UpdateLabels()
    {
        _NameLabel.Text = _inventoryItemWithQuantity.Item.GetName();
        if (_inventoryItemWithQuantity.Quantity > 1)
        {
            _NameLabel.Text += $" ({_inventoryItemWithQuantity.Quantity})";
        }
        _WeightLabel.Text = $"Weight: {_inventoryItemWithQuantity.Item.GetWeight()}";
        if (_inventoryItemWithQuantity.Quantity > 1)
        {
            _WeightLabel.Text +=
                $" ({_inventoryItemWithQuantity.Quantity * _inventoryItemWithQuantity.Item.GetWeight()})";
        }
    }

    private void setSelectedIndicatorAlpha(float alpha)
    {
        _SelectedIndicator.Modulate = new Color(
            _SelectedIndicator.Modulate.R,
            _SelectedIndicator.Modulate.G,
            _SelectedIndicator.Modulate.B,
            alpha
        );
    }
}
