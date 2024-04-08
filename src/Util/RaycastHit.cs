using Godot;
using Godot.Collections;

public class RaycastHit
{
    public Vector3 Position { get; }
    public Vector3 Normal { get; }
    public GodotObject Collider { get; }

    public RaycastHit(Dictionary collisionDict)
    {
        Position = collisionDict["position"].As<Vector3>();
        Normal = collisionDict["normal"].As<Vector3>();
        Collider = collisionDict["collider"].As<GodotObject>();
    }
}
