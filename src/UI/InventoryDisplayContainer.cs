using System;
using Godot;

public partial class InventoryDisplayContainer : SubViewportContainer
{
    [Export]
    private SimpleOrbitCamera _OrbitCamera;

    [Export]
    private Node3D _DisplayModel;

    private SubViewport _subViewport;

    private bool _mouseInArea = false;
    private bool _rotating = false;

    public override void _Ready()
    {
        base._Ready();
        _subViewport = GetChild<SubViewport>(0);

        MouseEntered += () =>
        {
            _mouseInArea = true;
        };
        MouseExited += () =>
        {
            _mouseInArea = false;
        };
    }

    public void ChangeModel(PackedScene newDisplayModel)
    {
        var freeModel = _DisplayModel;
        freeModel.QueueFree();
        _DisplayModel = newDisplayModel.Instantiate<Node3D>();
        _subViewport.AddChild(_DisplayModel);
        _OrbitCamera.Reset();
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        if (_mouseInArea && GameManager.InputManager().GetFireHeldInput())
        {
            _rotating = true;
        }
        else if (!GameManager.InputManager().GetFireHeldInput())
        {
            _rotating = false;
        }
        if (_rotating)
        {
            _OrbitCamera.Rotate(-GameManager.InputManager().GetAndResetMouseMotion(), delta);
        }
    }
}
