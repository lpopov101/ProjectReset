using System;
using Godot;

public partial class ItemPickup : Node3D
{
    [Export]
    private InventoryItem _inventoryItem;

    [Export]
    private AudioStream _pickupSound;

    private InteractionPoint _interactionPoint;

    public override void _Ready()
    {
        base._Ready();
        _interactionPoint = GetNode<InteractionPoint>($"{GetPath()}/InteractionPoint");
        if (_interactionPoint == null)
        {
            _interactionPoint = new InteractionPoint();
            _interactionPoint.SetPrompt("Pick Up");
        }
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
