using System;
using Godot;

public interface IInteractable
{
    public void Interact();
    public string GetInteractPrompt();
}
