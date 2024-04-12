using System;
using Godot;

public partial class ProjectileLauncher : Node3D
{
    enum State
    {
        Ready,
        Fire,
        Cooldown
    }

    [Export]
    private PackedScene _SpawnTemplate;

    [Export]
    private AudioStream _SoundEffect;

    [Export]
    private bool _RapidFire = false;

    [Export]
    private float _FireCooldownSecs = 1.0F;
    private Timer _cooldownTimer;
    private StateMachine<State> _stateMachine;

    public override void _Ready()
    {
        _cooldownTimer = new Timer { Autostart = false, OneShot = true };
        AddChild(_cooldownTimer);
        _stateMachine = new StateMachine<State>(State.Ready);

        _stateMachine.addStateProcessAction(
            State.Fire,
            () =>
            {
                LaunchProjectile();
                _cooldownTimer.Start(_FireCooldownSecs);
            }
        );

        _stateMachine.addStateTransition(
            State.Ready,
            State.Fire,
            () =>
            {
                return _RapidFire
                    ? Locator<InputManager>.Get().GetFireHeldInput()
                    : Locator<InputManager>.Get().GetFirePressedInput();
            }
        );
        _stateMachine.addStateTransition(
            State.Fire,
            State.Cooldown,
            () =>
            {
                return true;
            }
        );
        _stateMachine.addStateTransition(
            State.Cooldown,
            State.Ready,
            () =>
            {
                return _cooldownTimer.TimeLeft == 0;
            }
        );
    }

    public override void _Process(double delta)
    {
        GlobalRotation = GetParent<Node3D>().GlobalRotation;
        _stateMachine.ProcessState();
    }

    private void LaunchProjectile()
    {
        if (_SoundEffect != null)
        {
            Locator<SoundManager>
                .Get()
                .Spawn3DAudioAsChild(_SoundEffect, this)
                .WithVolume(-35F)
                .WithPitchVariation(0.2F)
                .Play();
        }
        Locator<SpawnManager>
            .Get()
            .GetPool(_SpawnTemplate)
            .Spawn3D<Hitscan>(GlobalPosition, GlobalRotation);
    }
}
