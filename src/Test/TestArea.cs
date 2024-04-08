using System;
using Godot;

public partial class TestArea : Area3D
{
    private MessageManager _messageManager;
    private RandomNumberGenerator _rng;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        _rng = new RandomNumberGenerator();
        BodyEntered += (body) =>
        {
            if (body.IsInGroup("Player"))
            {
                GameManager
                    .MessageManager()
                    .AddMessage($"Random Number: {_rng.RandiRange(0, 100)}");
            }
        };
    }
}
