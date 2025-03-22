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
    private float _SmoothingCoeff = 2.0F;
    [Export]
    private float _LedgeBoost = 1.0F;
    [Export]
    private float _MaxDistanceFromPlayer = 5.0F;

    private Vector3 _lastTargetVelocity = Vector3.Zero;
    private StateMachine<State> _stateMachine = new StateMachine<State>(State.Idle);
    private AnimationPlayer _animationPlayer;
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
        _stateMachine.addStateEnterAction(State.Idle, () =>
        {
            Velocity = Vector3.Zero;
            _animationPlayer.Pause();
        });
        _stateMachine.addStateEnterAction(State.Moving, () =>
        {
            _animationPlayer.Play("Walk", customBlend: 0.2F);
        });
        _stateMachine.addStateProcessAction(State.Moving, moveTowardsPlayer);
        _stateMachine.addStateTransition(State.Idle, State.Moving, () =>
        {
            return GlobalPosition.DistanceTo(getPlayerPosition()) > _MaxDistanceFromPlayer;
        });
        _stateMachine.addStateTransition(State.Moving, State.Idle, () =>
        {
            return GlobalPosition.DistanceTo(getPlayerPosition()) <= _MaxDistanceFromPlayer;
        });
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        _stateMachine.ProcessState(delta);
    }

    private void moveTowardsPlayer(double delta)
    {
        setTargetPosition(getPlayerPosition());
        Velocity = getTargetVelocity();
        Locator<MessageManager>.Get().AddMessage($"Delta: {getNextPathPosDelta()}");
        if (Velocity.Y > 0.01 && IsOnWall())
        {
            Velocity = new Vector3(Velocity.X, Velocity.Y + _LedgeBoost, Velocity.Z);
        }
        Velocity = _lastTargetVelocity.Lerp(Velocity, _SmoothingCoeff * (float)delta);
        _lastTargetVelocity = Velocity;
        MoveAndSlide();
        var lookTarget = new Vector3(
            GlobalPosition.X + _lastTargetVelocity.X,
            GlobalPosition.Y,
            GlobalPosition.Z + _lastTargetVelocity.Z
        );
        Transform = Transform.LookingAt(lookTarget, Vector3.Up);
    }

    private Vector3 getPlayerPosition()
    {
        return Locator<PlayerManager>
            .Get()
            .Player1()
            .GetCharacterBody()
            .GlobalPosition;
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
