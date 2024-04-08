using System;
using Godot;

public partial class MessageDisplayText : RichTextLabel
{
    public override void _Ready()
    {
        base._Ready();
        BbcodeEnabled = true;
    }

    public override void _Process(double delta)
    {
        Text = $"[center]{String.Join("\n", GameManager.MessageManager().GetMessages())}[/center]";
    }
}
