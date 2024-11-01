using System;
using Godot;

public partial class Door : Node3D
{
    private RigidBody3D _panel;
    private HingeJoint3D _hingeJoint;
    private InteractionPoint _frontInteractionPoint;
    private InteractionPoint _backInteractionPoint;
    private bool _opened = false;
    private double _targetYRotation;

    public override void _Ready()
    {
        base._Ready();
        _panel = GetNode<RigidBody3D>($"{GetPath()}/Panel");
        _hingeJoint = GetNode<HingeJoint3D>($"{GetPath()}/Hinge");

        _frontInteractionPoint = GetNode<InteractionPoint>($"{_panel.GetPath()}/FrontInteractionPoint");
        if (_frontInteractionPoint == null)
        {
            _frontInteractionPoint = new InteractionPoint();
            _frontInteractionPoint.SetPrompt("Open");
            _panel.AddChild(_frontInteractionPoint);
        }

        _backInteractionPoint = GetNode<InteractionPoint>($"{_panel.GetPath()}/BackInteractionPoint");
        if (_backInteractionPoint == null)
        {
            _backInteractionPoint = new InteractionPoint();
            _backInteractionPoint.SetPrompt("Open");
            _panel.AddChild(_backInteractionPoint);
        }

        _frontInteractionPoint.Connect(
            InteractionPoint.SignalName.OnInteract,
            new Callable(this, nameof(ToggleOpenClosed))
        );
        _targetYRotation = Rotation.Y;

        // _panel.BodyEntered += (body) =>
        // {
        //     Locator<MessageManager>.Get().AddMessage("Hello");
        //     if (body is PhysicsBody3D physicsBody)
        //     {
        //         Locator<MessageManager>.Get().AddMessage("Hello");
        //         _panel.ApplyImpulse(_panel.GlobalPosition - physicsBody.GlobalPosition);
        //     }
        // };
        // _panel.ApplyImpulse(new Vector3(1, 0, 1));
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        SelectInteractionPoint();
    }

    private void SelectInteractionPoint()
    {
        var playerPosition = Locator<PlayerManager>.Get().Player1().GetCharacterBody().GlobalPosition;
        var frontDistance = playerPosition.DistanceTo(_frontInteractionPoint.GlobalPosition);
        var backDistance = playerPosition.DistanceTo(_backInteractionPoint.GlobalPosition);
        if (frontDistance < backDistance)
        {
            _frontInteractionPoint.SetEnabled(true);
            _backInteractionPoint.SetEnabled(false);
        }
        else
        {
            _frontInteractionPoint.SetEnabled(false);
            _backInteractionPoint.SetEnabled(true);
        }
    }

    private void ToggleOpenClosed()
    {
        _opened = !_opened;
    }

    private void Open()
    {
        _panel.RotateY(Mathf.Pi);
    }

    private void Close()
    {
        _panel.RotateY(-Mathf.Pi);
    }
}
