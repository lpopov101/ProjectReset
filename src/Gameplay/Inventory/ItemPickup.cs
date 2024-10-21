using System;
using Godot;

public partial class ItemPickup : Node3D
{
    [Export]
    private InventoryItem _inventoryItem;

    [Export]
    private string _InteractionPrompt = "Interact";

    [Export]
    private float _MaxInteractionHintDistance = 50F;

    [Export]
    private float _MaxInteractionDistance = 5F;

    private InteractionPoint _interactionPoint;

    public override void _Ready()
    {
        base._Ready();
        _interactionPoint = new InteractionPoint();
        _interactionPoint.SetUpPrompt(
            _InteractionPrompt,
            _MaxInteractionHintDistance,
            _MaxInteractionDistance
        );
        _interactionPoint.Connect(
            InteractionPoint.SignalName.OnInteract,
            new Callable(this, nameof(PickUp))
        );
        AddChild(_interactionPoint);
    }

    private void PickUp()
    {
        Locator<PlayerManager>.Get().Player1().GetInventory().TryAddItem(_inventoryItem);
        QueueFree();
    }
}
