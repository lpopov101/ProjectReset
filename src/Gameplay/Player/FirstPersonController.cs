using System;
using System.Runtime.InteropServices;
using System.Text;
using Godot;

[GlobalClass]
public partial class FirstPersonController : CharacterBody3D, IPickupable, IPlayer
{
    [Export]
    private float _MouseSensitivity = 0.2f;

    [Export]
    private Node3D _StairProbe;

    [Export]
    private WalkingCharacterSettings _WalkingCharacterSettings;
    private WalkingCharacterHandler _walkingCharacterHandler;
    private Camera3D _camera;
    private float _pitch = 0.0f;

    public override void _Ready()
    {
        SetCollisionLayerValue(PlayerManager.PLAYER_COLLISION_LAYER, true);
        _camera = GetViewport().GetCamera3D();

        _walkingCharacterHandler = new WalkingCharacterHandler();
        _walkingCharacterHandler.Init(this, _WalkingCharacterSettings, _StairProbe);
        AddChild(_walkingCharacterHandler);
    }

    public override void _Process(double delta)
    {
        _walkingCharacterHandler.SetMovementInput(
            new Vector2(
                Locator<InputManager>.Get().GetHorizontalMovementInput(),
                Locator<InputManager>.Get().GetVerticalMovementInput()
            )
        );
        _walkingCharacterHandler.SetJumpInput(Locator<InputManager>.Get().GetJumpInput());

        var mouseMotion = Locator<InputManager>.Get().GetAndResetMouseMotion();
        _walkingCharacterHandler.SetRotationInput(-mouseMotion.X * _MouseSensitivity);
        _pitch += Mathf.DegToRad(-mouseMotion.Y * _MouseSensitivity * (float)delta);
        _pitch = Mathf.Clamp(_pitch, Mathf.DegToRad(-90.0f), Mathf.DegToRad(90.0f));
        _camera.Rotation = new Vector3(_pitch, 0, 0);
    }

    public void PickUp(string message)
    {
        Locator<MessageManager>.Get().AddMessage(message);
    }

    public Player CreatePlayer()
    {
        return new Player(this, GetViewport().GetCamera3D());
    }
}
