using System;
using System.Collections.Generic;
using Godot;

public partial class SpawnManager : Node
{
    private Dictionary<PackedScene, Pool> _pools;

    public Pool GetPool(PackedScene packedScene, int initPoolSize = 128)
    {
        if (_pools == null)
        {
            _pools = new Dictionary<PackedScene, Pool>();
        }
        if (!_pools.ContainsKey(packedScene))
        {
            var pool = Pool.Create(packedScene, initPoolSize);
            AddChild(pool);
            _pools[packedScene] = pool;
        }
        return _pools[packedScene];
    }
}
