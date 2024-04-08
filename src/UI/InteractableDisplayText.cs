using System;
using Godot;

public partial class InteractableDisplayText : RichTextLabel
{
    public override void _Process(double delta)
    {
        var interactable = InteractionPoint.GetCurInteractablePoint();
        if (interactable != null)
        {
            Text = $"[center]Press interact key to {interactable.GetInteractionPrompt()}[/center]";
        }
        else
        {
            Text = "";
        }
    }
}
