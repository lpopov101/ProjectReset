using System;
using System.Collections.Generic;
using Godot;

public partial class PlayerManager : Node
{
    private List<Player> _players;

    public override void _Ready()
    {
        _players = new List<Player>();
        ScanForPlayers(GetTree().Root);
        // var playerNodes = GetTree().GetNodesInGroup("Player");
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

    public Player GetPlayer1()
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
}
