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

    [Export]
    private AudioStream _pickupSound;

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
        if (_pickupSound != null)
        {
            Locator<SoundManager>
                .Get()
                .Spawn3DAudioAsSibling(_pickupSound, this)
                .WithPitchVariation(0.1F)
                .PlayOnce();
        }
        _interactionPoint.setEnabled(false);
        QueueFree();
    }
}
