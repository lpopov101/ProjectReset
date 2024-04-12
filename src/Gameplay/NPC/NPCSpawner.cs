using System;
using Godot;

public partial class NPCSpawner : Node3D
{
    [Export]
    public PackedScene _NPCTemplate;

    [Export]
    public bool _OneAtATime;

    [Export]
    public bool _SpawnOnStart;

    private bool _canSpawn = true;
    private NPC _curSpawned;

    public override void _Ready()
    {
        base._Ready();
        if (_SpawnOnStart)
        {
            CallDeferred(nameof(Spawn));
        }
    }

    public void Spawn()
    {
        if (!_OneAtATime || _curSpawned == null || !_curSpawned._IsSpawned)
        {
            _curSpawned = Locator<SpawnManager>
                .Get()
                .GetPool(_NPCTemplate)
                .Spawn3D<NPC>(GlobalPosition, GlobalRotation);
        }
    }
}
