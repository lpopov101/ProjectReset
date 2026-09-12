using System;
using System.Collections;
using System.Collections.Generic;
using Godot;

public partial class Pool : Node
{
    public enum SpawnCoordsMode
    {
        Local,
        Global
    }

    [Export]
    private PackedScene _Template;

    [Export]
    private int _PoolSize = 128;
    private Queue _poolQueue;
    private Queue _despawnQueue;
    private Node _sceneRoot;

    // Instances currently parked in _poolQueue, and instances whose despawn is
    // queued but not applied yet. One object can ask to despawn more than once
    // in a single frame - two BodyEntered signals out of one physics step, or a
    // collision racing the lifespan timer - and without these guards the second
    // request parks it a second time, after which the pool hands the same
    // instance to two callers at once.
    private readonly HashSet<ISpawnable> _pooled = new HashSet<ISpawnable>(
        ReferenceEqualityComparer.Instance
    );
    private readonly HashSet<ISpawnable> _pendingDespawn = new HashSet<ISpawnable>(
        ReferenceEqualityComparer.Instance
    );

    public static Pool Create(PackedScene templateScene, int initPoolSize = 128)
    {
        var result = new Pool { _Template = templateScene, _PoolSize = initPoolSize };
        return result;
    }

    public static Pool Create(Node templateNode, int initPoolSize = 128)
    {
        var templateScene = new PackedScene();
        templateScene.Pack(templateNode);
        return Create(templateScene, initPoolSize);
    }

    public override void _Ready()
    {
        base._Ready();
        _poolQueue = new Queue();
        _despawnQueue = new Queue();
        _sceneRoot = GetTree().Root;
        populatePool();
    }

    public T SpawnAsChild3D<T>(
        Node parent,
        Vector3 position,
        Vector3 rotation,
        SpawnCoordsMode spawnCoordsMode = SpawnCoordsMode.Global
    )
        where T : Node3D, ISpawnable
    {
        var result = spawnHelper<T>();
        parent.AddChild(result);
        if (spawnCoordsMode == SpawnCoordsMode.Global)
        {
            result.GlobalPosition = position;
            result.GlobalRotation = rotation;
        }
        else if (spawnCoordsMode == SpawnCoordsMode.Local)
        {
            result.Position = position;
            result.Rotation = rotation;
        }
        if (result is RigidBody3D rb)
        {
            rb.Freeze = false;
        }
        result.OnSpawn();
        return result;
    }

    public T SpawnAsChild3D<T>(
        Node parent,
        Transform3D transform,
        SpawnCoordsMode spawnCoordsMode = SpawnCoordsMode.Global
    )
        where T : Node3D, ISpawnable
    {
        var result = spawnHelper<T>();
        parent.AddChild(result);
        if (spawnCoordsMode == SpawnCoordsMode.Global)
        {
            result.GlobalTransform = transform;
        }
        else if (spawnCoordsMode == SpawnCoordsMode.Local)
        {
            result.Transform = transform;
        }
        if (result is RigidBody3D rb)
        {
            rb.Freeze = false;
        }
        result.OnSpawn();
        return result;
    }

    public T SpawnAsSibling3D<T>(
        Node sibling,
        Vector3 position,
        Vector3 rotation,
        SpawnCoordsMode spawnCoordsMode = SpawnCoordsMode.Global
    )
        where T : Node3D, ISpawnable
    {
        return SpawnAsChild3D<T>(sibling.GetParent(), position, rotation, spawnCoordsMode);
    }

    public T Spawn3D<T>(
        Vector3 position,
        Vector3 rotation,
        SpawnCoordsMode spawnCoordsMode = SpawnCoordsMode.Global
    )
        where T : Node3D, ISpawnable
    {
        return SpawnAsChild3D<T>(_sceneRoot, position, rotation, spawnCoordsMode);
    }

    public T Spawn3D<T>(
        Transform3D transform,
        SpawnCoordsMode spawnCoordsMode = SpawnCoordsMode.Global
    )
        where T : Node3D, ISpawnable
    {
        return SpawnAsChild3D<T>(_sceneRoot, transform, spawnCoordsMode);
    }

    public T Spawn3D<T>(SpawnCoordsMode spawnCoordsMode = SpawnCoordsMode.Global)
        where T : Node3D, ISpawnable
    {
        return SpawnAsChild3D<T>(_sceneRoot, new Transform3D(), spawnCoordsMode);
    }

    public T SpawnAsChild<T>(Node parent)
        where T : Node, ISpawnable
    {
        var result = spawnHelper<T>();
        parent.AddChild(result);
        result.OnSpawn();
        return result;
    }

    public T SpawnAsSibling<T>(Node sibling)
        where T : Node, ISpawnable
    {
        return SpawnAsChild<T>(sibling.GetParent());
    }

    public T Spawn<T>()
        where T : Node, ISpawnable
    {
        return SpawnAsChild<T>(_sceneRoot);
    }

    public static void DespawnStatic<T>(T despawnObj)
        where T : ISpawnable
    {
        despawnObj.OriginPool.QueueDespawn(despawnObj);
    }

    public void Despawn<T>(T despawnObj)
        where T : ISpawnable
    {
        _pendingDespawn.Remove(despawnObj);
        if (!_pooled.Add(despawnObj))
        {
            // Already parked, so this is a repeat request for the same life.
            return;
        }
        despawnObj.OnDespawn();
        _poolQueue.Enqueue(despawnObj);
        if (despawnObj is Node node)
        {
            node.GetParent()?.RemoveChild(node);
            node.SetProcess(false);
        }
        if (despawnObj is Node3D node3D)
        {
            node3D.Visible = false;
        }
        if (despawnObj is RigidBody3D rb)
        {
            rb.Freeze = true;
        }
    }

    public void QueueDespawn<T>(T despawnObj)
        where T : ISpawnable
    {
        if (_pooled.Contains(despawnObj) || !_pendingDespawn.Add(despawnObj))
        {
            return;
        }
        Action despawnAction = () =>
        {
            Despawn(despawnObj);
        };
        _despawnQueue.Enqueue(despawnAction);
    }

    public override void _Process(double delta)
    {
        while (_despawnQueue.Count > 0)
        {
            var despawnAction = _despawnQueue.Dequeue() as Action;
            despawnAction();
        }
    }

    private T spawnHelper<T>()
        where T : Node, ISpawnable
    {
        var result = fetchInstance<T>();
        if (result is Node3D node3D)
        {
            node3D.Visible = true;
        }
        result.SetProcess(true);
        return result;
    }

    private T fetchInstance<T>()
        where T : Node, ISpawnable
    {
        if (_poolQueue == null)
        {
            _poolQueue = new Queue();
        }
        if (_poolQueue.Count == 0)
        {
            extendPool();
        }
        var instance = _poolQueue.Dequeue();
        if (instance is ISpawnable spawnable)
        {
            _pooled.Remove(spawnable);
        }
        if (instance is T result)
        {
            return result;
        }
        return null;
    }

    private void populatePool()
    {
        for (int i = 0; i < _PoolSize; ++i)
        {
            var poolItem = _Template.Instantiate();
            if (poolItem is ISpawnable spawnable)
            {
                spawnable.OriginPool = this;
                _pooled.Add(spawnable);
            }
            if (poolItem is Node3D node3D)
            {
                node3D.Visible = false;
            }
            if (poolItem is RigidBody3D rb)
            {
                rb.Freeze = true;
            }
            poolItem.SetProcess(false);
            _poolQueue.Enqueue(poolItem);
        }
    }

    private void extendPool()
    {
        populatePool();
        _PoolSize *= 2;
    }
}
