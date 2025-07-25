using System;
using Godot;

public partial class TestNPC : NPC
{
    enum State
    {
        Idle,
        Moving
    }

    [Export]
    private float _MaxDistanceFromPlayer = 5.0F;

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
        var movementDirectionXZ = new Vector3(
            movementDirection.X,
            0,
            movementDirection.Z
        ).Normalized();

        var forward = GlobalTransform.Basis.Z.Normalized();
        var angleToDirection = -forward.SignedAngleTo(movementDirectionXZ, Vector3.Up);
        Locator<MessageManager>
            .Get()
            .AddMessage($"Angle to direction: {Mathf.RadToDeg(angleToDirection)}");
        _walkingCharacterHandler.SetRotationInput(Mathf.Sign(angleToDirection) * 100F);
        _walkingCharacterHandler.SetMovementInput(new Vector2(0, -1));
    }

    private Vector3 getPlayerPosition()
    {
        return Locator<PlayerManager>.Get().Player1().GetCharacterBody().GlobalPosition;
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
