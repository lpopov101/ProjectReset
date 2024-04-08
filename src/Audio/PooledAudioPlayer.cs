using System;
using Godot;

public partial class PooledAudioPlayer : AudioStreamPlayer, ISpawnable
{
    enum PlayState
    {
        PlayingOnce,
        Looping,
        Waiting,
        Done
    }

    private PlayState _playState = PlayState.Waiting;
    private bool _reclaimWhenDone = false;
    private RandomNumberGenerator _rng;
    public Pool OriginPool { get; set; }

    public override void _Ready()
    {
        base._Ready();
        _rng = new RandomNumberGenerator();
    }

    public PooledAudioPlayer WithPitchVariation(float variation)
    {
        PitchScale = _rng.RandfRange(1F - variation, 1F + variation);
        return this;
    }

    public PooledAudioPlayer WithVolume(float volume)
    {
        VolumeDb = volume;
        return this;
    }

    public void PlayOnce()
    {
        _playState = PlayState.PlayingOnce;
        Play();
    }

    public void Loop()
    {
        _playState = PlayState.Looping;
        Play();
    }

    public void Finish()
    {
        _playState = PlayState.Done;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        base._Process(delta);
        if (!Playing)
        {
            if (_playState == PlayState.PlayingOnce)
            {
                _playState = PlayState.Done;
            }
            else if (_playState == PlayState.Looping)
            {
                Play();
            }
        }
        if (_playState == PlayState.Done)
        {
            ((ISpawnable)this).Despawn();
        }
    }

    public void OnSpawn() { }

    public void OnDespawn()
    {
        _playState = PlayState.Waiting;
        PitchScale = 1F;
        VolumeDb = 0F;
    }
}
