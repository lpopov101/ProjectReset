using System;
using Godot;

public partial class SoundManager : Node
{
    private Pool _audioPlayerPool;
    private Pool _audioPlayer3DPool;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        _audioPlayerPool = Pool.Create(new PooledAudioPlayer());
        AddChild(_audioPlayerPool);
        _audioPlayer3DPool = Pool.Create(new PooledAudioPlayer3D());
        AddChild(_audioPlayer3DPool);
    }

    public PooledAudioPlayer3D Spawn3DAudioAsChild(AudioStream stream, Node3D parent)
    {
        var audioPlayer3D = _audioPlayer3DPool.SpawnAsChild3D<PooledAudioPlayer3D>(
            parent,
            parent.GlobalPosition,
            Vector3.Zero
        );
        audioPlayer3D.Stream = stream;
        return audioPlayer3D;
    }

    public PooledAudioPlayer3D Spawn3DAudioAsSibling(AudioStream stream, Node3D sibling)
    {
        var audioPlayer3D = _audioPlayer3DPool.SpawnAsSibling3D<PooledAudioPlayer3D>(
            sibling,
            sibling.GlobalPosition,
            Vector3.Zero
        );
        audioPlayer3D.Stream = stream;
        return audioPlayer3D;
    }

    public PooledAudioPlayer3D Spawn3DAudio(
        AudioStream stream,
        Vector3 position,
        Pool.SpawnCoordsMode spawnCoordsMode = Pool.SpawnCoordsMode.Global
    )
    {
        var audioPlayer3D = _audioPlayer3DPool.Spawn3D<PooledAudioPlayer3D>(
            position,
            Vector3.Zero,
            spawnCoordsMode
        );
        audioPlayer3D.Stream = stream;
        return audioPlayer3D;
    }

    public PooledAudioPlayer SpawnAudio(AudioStream stream)
    {
        var audioStreamplayer = _audioPlayerPool.Spawn<PooledAudioPlayer>();
        audioStreamplayer.Stream = stream;
        return audioStreamplayer;
    }
}
