using System;
using System.Collections.Generic;
using Godot;

public partial class PlayerManager : Node
{
    public static int PLAYER_COLLISION_LAYER = 2;
    private List<Player> _players;

    public override void _Ready()
    {
        base._Ready();
        Locator<PlayerManager>.Register(this);
        _players = new List<Player>();
        scanForPlayers(GetTree().Root);
    }

    public Player Player1()
    {
        return GetPlayer(0);
    }

    public Player GetPlayer(int index)
    {
        if (_players.Count > index)
        {
            return _players[index];
        }
        return null;
    }

    private void scanForPlayers(Node node)
    {
        if (node is IPlayer player)
        {
            _players.Add(player.CreatePlayer());
        }
        foreach (var child in node.GetChildren())
        {
            scanForPlayers(child);
        }
    }
}
