using System;
using Godot;

public partial class TestItemPickup : Area3D
{
    [Export]
    private AudioStream _pickupSound;

    [Export]
    private float _rotateSpeed = 5F;

    [Export]
    private float _distanceFromGround = 0.3F;

    [Export]
    private InventoryItem _inventoryItem;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        var groundY = findGroundY();
        if (groundY != float.NaN)
        {
            Position = new Vector3(
                Transform.Origin.X,
                groundY + _distanceFromGround,
                Transform.Origin.Z
            );
        }
        BodyEntered += (body) =>
        {
            if (body is IPickupable pickupable)
            {
                GameManager.Player1().GetInventory().TryAddItem(_inventoryItem);
                pickupable.PickUp($"Picked up {_inventoryItem.GetName()}");
                if (_pickupSound != null)
                {
                    GameManager
                        .SoundManager()
                        .Spawn3DAudioAsSibling(_pickupSound, this)
                        .WithPitchVariation(0.1F)
                        .PlayOnce();
                }
                QueueFree();
            }
        };
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        RotateY(_rotateSpeed * (float)delta);
    }

    private float findGroundY()
    {
        var rayCastHit = new RaycastBuilder(this)
            .FromPosition(GlobalPosition)
            .WithDirectionMode(RaycastBuilder.DirectionMode.Global)
            .WithDirectionAndMagnitude(Vector3.Down, 1000F)
            .Cast();
        if (rayCastHit != null)
        {
            return rayCastHit.Position.Y;
        }
        return float.NaN;
    }
}
