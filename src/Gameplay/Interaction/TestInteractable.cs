using System;
using Godot;

public partial class TestInteractable : StaticBody3D, IInteractable
{
    [Signal]
    public delegate void InteractedEventHandler();

    [Export]
    public AudioStream _InteractSoundEffect;

    public string GetInteractPrompt()
    {
        return $"interact With {Name}";
    }

    public void Interact()
    {
        GameManager.MessageManager().AddMessage($"Interacted with {Name}");
        GameManager.SoundManager().Spawn3DAudioAsChild(_InteractSoundEffect, this).PlayOnce();
        EmitSignal(SignalName.Interacted);
    }
}
