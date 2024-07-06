using System;
using Godot;

public partial class InventoryItemView : Control
{
    public delegate void ButtonEventHandler(InventoryItem inventoryItem);

    public event ButtonEventHandler Use;
    public event ButtonEventHandler Discard;

    [Export]
    private InventoryDisplayContainer _InventoryDisplayContainer;

    [Export]
    private RichTextLabel _NameLabel;

    [Export]
    private RichTextLabel _DescriptionLabel;

    [Export]
    private Button _UseButton;

    [Export]
    private Button _DiscardButton;
    private InventoryItem _item;

    public override void _Ready()
    {
        base._Ready();
        _NameLabel.BbcodeEnabled = true;
        _DescriptionLabel.BbcodeEnabled = true;
        ClearItem();

        _UseButton.Pressed += () =>
        {
            Use.Invoke(_item);
        };

        _DiscardButton.Pressed += () =>
        {
            Discard.Invoke(_item);
        };
    }

    public void SetItem(InventoryItem item)
    {
        _item = item;
        _InventoryDisplayContainer.ChangeModel(item.GetDisplayModelTemplate());
        _NameLabel.Text = $"[center]{item.GetName()}[/center]";
        _DescriptionLabel.Text = item.GetDescription();
        _UseButton.Visible = true;
        _DiscardButton.Visible = true;
        _UseButton.Text = item.GetUseActionName();
    }

    public void ClearItem()
    {
        _item = null;
        var displayModelTemplate = new PackedScene();
        displayModelTemplate.Pack(new Node3D());
        _InventoryDisplayContainer.ChangeModel(displayModelTemplate);
        _NameLabel.Text = "";
        _DescriptionLabel.Text = "";
        _UseButton.Visible = false;
        _DiscardButton.Visible = false;
    }
}
