using System;
using Godot;

public interface ISpawnable
{
    public Pool OriginPool { get; set; }
    public void OnSpawn();
    public void OnDespawn();

    public void Despawn()
    {
        Pool.DespawnStatic(this);
    }
}
