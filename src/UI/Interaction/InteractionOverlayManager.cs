using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Godot;

[GlobalClass]
public partial class InteractionOverlayManager : Control
{
    [Export]
    private PackedScene _InteractionOverlayTemplate;

    [Export(PropertyHint.Range, "0,1,")]
    private float _MaxHintAlpha = 0.3F;

    [Export]
    private bool _ShowKeyPrompts = true;

    private Dictionary<InteractionPoint, InteractionOverlay> _interactionPointOverlayDict;
    private Pool _interactionPointPool;

    public override void _Ready()
    {
        base._Ready();
        _interactionPointPool = Locator<SpawnManager>.Get().GetPool(_InteractionOverlayTemplate);
        _interactionPointOverlayDict = new Dictionary<InteractionPoint, InteractionOverlay>();

        InteractionPoint.VisibleInteractionPointAdded += (interactionPoint) =>
        {
            var interactionOverlay = _interactionPointPool.SpawnAsChild<InteractionOverlay>(this);
            _interactionPointOverlayDict[interactionPoint] = interactionOverlay;
        };

        InteractionPoint.VisibleInteractionPointRemoved += (interactionPoint) =>
        {
            var interactionOverlay = _interactionPointOverlayDict[interactionPoint];
            ((ISpawnable)interactionOverlay).Despawn();
            _interactionPointOverlayDict.Remove(interactionPoint);
        };
    }

    public override void _Process(double delta)
    {
        foreach (var entry in _interactionPointOverlayDict)
        {
            var interactionPoint = entry.Key;
            var interactionOverlay = entry.Value;

            interactionOverlay.Position = interactionPoint.GetScreenCoords();
            if (interactionPoint.IsCurInteractablePoint())
            {
                interactionOverlay.SetAlpha(1F);
                interactionOverlay.DisplayMessage(
                    (_ShowKeyPrompts ? (Locator<InputManager>.Get().GetInteractKeys() + ": ") : "")
                        + interactionPoint.GetInteractionPrompt()
                );
            }
            else
            {
                interactionOverlay.SetAlpha(
                    interactionPoint.GetNormalizedDistanceToCamera() * _MaxHintAlpha
                );
                interactionOverlay.HideMessage();
            }
        }
    }
}
