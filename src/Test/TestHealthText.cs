using System;
using Godot;

public partial class TestHealthText : RichTextLabel
{
    [Export]
    private Camera3D _Camera;

    [Export]
    private Node3D _Damageable;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready() { }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        if (_Camera.IsPositionBehind(_Damageable.GlobalPosition))
        {
            Visible = false;
        }
        else
        {
            Visible = true;
            Position =
                _Camera.UnprojectPosition(_Damageable.GlobalPosition)
                + GetViewportRect().Size.Y * 0.1F * Vector2.Up;
            if (_Damageable is IDamageable damageable)
            {
                Text = "" + damageable._Health;
            }
        }
    }
}
