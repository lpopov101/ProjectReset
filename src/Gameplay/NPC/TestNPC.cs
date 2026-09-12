using System;
using Godot;

public partial class TestNPC : NPC, IDoorOpener, IDoorCloser
{
    enum State
    {
        Idle,
        Moving
    }

    [Export]
    private float _MaxDistanceFromPlayer = 5.0F;

    [Export]
    private bool _CanOpenDoors = false;

    [Export]
    private bool _CanCloseDoors = false;

    [Export]
    private WalkingCharacterSettings _walkingCharacterSettings;

    private Vector3 _lastTargetVelocity = Vector3.Zero;
    private StateMachine<State> _stateMachine = new StateMachine<State>(State.Idle);
    private AnimationPlayer _animationPlayer;
    private WalkingCharacterHandler _walkingCharacterHandler;

    // private MeshInstance3D _mesh;
    // private StandardMaterial3D _material;

    public override void _Ready()
    {
        base._Ready();
        // _mesh = GetNode<MeshInstance3D>("MeshInstance3D");
        // _material = _mesh.GetSurfaceOverrideMaterial(0) as StandardMaterial3D;
        _Health = _MaxHealth;
        _animationPlayer = GetNode<AnimationPlayer>("Model/AnimationPlayer");
        var walkAnimation = _animationPlayer.GetAnimation("Walk");
        walkAnimation.LoopMode = Animation.LoopModeEnum.Linear;
        _stateMachine.addStateEnterAction(
            State.Idle,
            () =>
            {
                _walkingCharacterHandler.SetMovementInput(Vector2.Zero);
                _walkingCharacterHandler.SetRotationInput(0F);
                _animationPlayer.Pause();
            }
        );
        _stateMachine.addStateEnterAction(
            State.Moving,
            () =>
            {
                _animationPlayer.Play("Walk", customBlend: 0.2F);
            }
        );
        _stateMachine.addStateProcessAction(State.Moving, moveTowardsPlayer);
        _stateMachine.addStateTransition(
            State.Idle,
            State.Moving,
            () =>
            {
                return GlobalPosition.DistanceTo(getPlayerPosition()) > _MaxDistanceFromPlayer;
            }
        );
        _stateMachine.addStateTransition(
            State.Moving,
            State.Idle,
            () =>
            {
                return GlobalPosition.DistanceTo(getPlayerPosition()) <= _MaxDistanceFromPlayer;
            }
        );
        _walkingCharacterHandler = new WalkingCharacterHandler();
        _walkingCharacterHandler.Init(
            this,
            _walkingCharacterSettings,
            GetNode<Node3D>("StairProbe")
        );
        AddChild(_walkingCharacterHandler);
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        _stateMachine.ProcessState(delta);
    }

    private void moveTowardsPlayer(double delta)
    {
        setTargetPosition(getPlayerPosition());
        var movementDirection = getMovementDirection();
        var localMovementDirection = ToLocal(GlobalPosition + movementDirection);
        var rotationDirectionMultiplier = -Mathf.Sign(localMovementDirection.X);
        var angleToTarget = Mathf.RadToDeg(
            Vector3.Forward.AngleTo(
                new Vector3(localMovementDirection.X, 0, localMovementDirection.Z)
            )
        );
        var dampingFactor = Mathf.Sqrt(
            Mathf.Clamp(Mathf.InverseLerp(0F, 180F, Mathf.Abs(angleToTarget)), 0F, 1F)
        );
        var rotationInput = 500 * rotationDirectionMultiplier * dampingFactor;
        var movementSpeed = 1 - dampingFactor;
        _walkingCharacterHandler.SetRotationInput(rotationInput);
        _walkingCharacterHandler.SetMovementInput(Vector2.Up * movementSpeed);
    }

    private Vector3 getPlayerPosition()
    {
        return Locator<PlayerManager>.Get().Player1().GetCharacterBody().GlobalPosition;
    }

    public bool CanOpenDoors()
    {
        return _CanOpenDoors;
    }

    public bool CanCloseDoors()
    {
        return _CanCloseDoors;
    }

    public override void Damage(
        float damage,
        IDamageable.DamageType damageType = IDamageable.DamageType.Regular
    )
    {
        _Health -= damage;
        // _material.AlbedoColor = new Color(1F, _Health / _MaxHealth, _Health / _MaxHealth);
        if (_Health <= 0)
        {
            ((ISpawnable)this).Despawn();
        }
    }

    public override void OnDespawn()
    {
        base.OnDespawn();
        // _material.AlbedoColor = new Color(1F, 1F, 1F);
    }
}
