using System;
using System.Collections.Generic;
using Godot;

public partial class GameManager : Node
{
    private static GameManager _instance;
    private InputManager _InputManager;
    private MessageManager _MessageManager;
    private SoundManager _SoundManager;

    private List<Player> _players;
    private Dictionary<PackedScene, Pool> _pools;

    public static InputManager InputManager()
    {
        return _instance._InputManager;
    }

    public static MessageManager MessageManager()
    {
        return _instance._MessageManager;
    }

    public static SoundManager SoundManager()
    {
        return _instance._SoundManager;
    }

    public static Player Player1()
    {
        return GetPlayer(0);
    }

    public static Player GetPlayer(int index)
    {
        if (_instance._players.Count > index)
        {
            return _instance._players[index];
        }
        return null;
    }

    public static Pool GetPool(PackedScene packedScene, int initPoolSize = 128)
    {
        if (_instance._pools == null)
        {
            _instance._pools = new Dictionary<PackedScene, Pool>();
        }
        if (!_instance._pools.ContainsKey(packedScene))
        {
            var pool = Pool.Create(packedScene, initPoolSize);
            pool.ProcessMode = ProcessModeEnum.Always;
            _instance.AddChild(pool);
            _instance._pools[packedScene] = pool;
        }
        return _instance._pools[packedScene];
    }

    public override void _Ready()
    {
        base._Ready();
        _InputManager = GetNode<InputManager>("InputManager");
        _MessageManager = GetNode<MessageManager>("MessageManager");
        _SoundManager = GetNode<SoundManager>("SoundManager");
        _players = new List<Player>();
        ScanForPlayers(GetTree().Root);
        _pools = new Dictionary<PackedScene, Pool>();
        _instance = this;
    }

    public void ScanForPlayers(Node node)
    {
        if (node is IPlayer player)
        {
            _players.Add(player.CreatePlayer());
        }
        foreach (var child in node.GetChildren())
        {
            ScanForPlayers(child);
        }
    }
}
