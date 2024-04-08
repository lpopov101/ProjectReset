using System;
using System.Collections.Generic;
using Godot;

public partial class MessageManager : Node
{
    [Export]
    private int _MessageCapacity;

    [Export]
    private double _MessageLifespanSecs;

    private Timer _messageExpiryTimer;
    private Queue<string> _messageQueue;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        _messageQueue = new Queue<string>();
        _messageExpiryTimer = new Timer { Autostart = true, OneShot = true };
        _messageExpiryTimer.Timeout += () =>
        {
            if (_messageQueue.Count > 0)
            {
                _messageQueue.Dequeue();
            }
            if (_messageQueue.Count > 0)
            {
                _messageExpiryTimer.Start(_MessageLifespanSecs);
            }
        };
        AddChild(_messageExpiryTimer);
    }

    public void AddMessage(string message)
    {
        while (_messageQueue.Count >= _MessageCapacity)
        {
            _messageQueue.Dequeue();
        }
        _messageQueue.Enqueue(message);
        _messageExpiryTimer.Start(_MessageLifespanSecs);
    }

    public string[] GetMessages()
    {
        return _messageQueue.ToArray();
    }
}
