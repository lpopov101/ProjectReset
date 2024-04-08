using System;
using Godot;

[GlobalClass]
public partial class InteractionOverlay : TextureRect, ISpawnable
{
    public Pool OriginPool { get; set; }

    [Export]
    private RichTextLabel _Text;

    public override void _Process(double delta)
    {
        base._Process(delta);
    }

    public override void _Ready()
    {
        base._Ready();
        SetAlpha(0);
        HideMessage();
    }

    public void OnSpawn() { }

    public void OnDespawn() { }

    public void DisplayMessage(string message)
    {
        _Text.Text = message;
    }

    public void HideMessage()
    {
        _Text.Text = "";
    }

    public void SetAlpha(float alpha)
    {
        Modulate = new Color(Modulate.R, Modulate.G, Modulate.B, alpha);
    }
}
