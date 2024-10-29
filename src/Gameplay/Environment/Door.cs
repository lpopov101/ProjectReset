using System;
using Godot;

public partial class Door : Node3D
{
    private InteractionPoint _frontInteractionPoint;
    private InteractionPoint _backInteractionPoint;

    public override void _Ready()
    {
        base._Ready();

        _frontInteractionPoint = GetNode<InteractionPoint>($"{GetPath()}/FrontInteractionPoint");
        if (_frontInteractionPoint == null)
        {
            _frontInteractionPoint = new InteractionPoint();
            _frontInteractionPoint.SetPrompt("Open");
        }

        _backInteractionPoint = GetNode<InteractionPoint>($"{GetPath()}/BackInteractionPoint");
        if (_backInteractionPoint == null)
        {
            _backInteractionPoint = new InteractionPoint();
            _backInteractionPoint.SetPrompt("Open");
        }
    }
}
