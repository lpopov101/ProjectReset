using System.Linq;
using Godot;
using Godot.Collections;

public class RaycastBuilder
{
    public enum DirectionMode
    {
        Local,
        Global
    }

    private Node3D _baseNode;
    private DirectionMode _directionMode = DirectionMode.Local;
    private Vector3 _from = Vector3.Zero;
    private Vector3 _to = Vector3.Zero;
    private Array<CollisionObject3D> _ignoreObjects;

    public RaycastBuilder(Node3D baseNode)
    {
        _baseNode = baseNode;
        _ignoreObjects = new Array<CollisionObject3D>();
    }

    public RaycastBuilder FromPosition(Vector3 from)
    {
        _from = from;
        return this;
    }

    public RaycastBuilder ToPosition(Vector3 to)
    {
        _to = to;
        return this;
    }

    public RaycastBuilder WithDirectionMode(DirectionMode directionMode)
    {
        _directionMode = directionMode;
        return this;
    }

    public RaycastBuilder WithDirectionAndMagnitude(Vector3 direction, float magnitude)
    {
        direction = direction.Normalized();
        if (_directionMode == DirectionMode.Local)
        {
            direction = _baseNode.GlobalBasis * direction;
        }
        _to = _from + (direction * magnitude);
        return this;
    }

    public RaycastBuilder WithIgnoredObject(CollisionObject3D ignoreObject)
    {
        _ignoreObjects.Add(ignoreObject);
        return this;
    }

    public RaycastBuilder WithIgnoredObjects(Array<CollisionObject3D> ignoreObjects)
    {
        foreach (var ignoreObject in ignoreObjects)
        {
            WithIgnoredObject(ignoreObject);
        }
        return this;
    }

    public RaycastHit Cast()
    {
        var worldSpace = _baseNode.GetWorld3D().DirectSpaceState;
        var rayQuery = PhysicsRayQueryParameters3D.Create(_from, _to);
        rayQuery.Exclude = new Array<Rid>(
            from collisionObject in _ignoreObjects
            select collisionObject.GetRid()
        );
        var collision = worldSpace.IntersectRay(rayQuery);
        if (collision.Count > 0)
        {
            return new RaycastHit(collision);
        }
        return null;
    }
}
