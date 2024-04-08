using Godot;
using Godot.Collections;

public class RaycastUtil
{
    public static RaycastHit castRay(Node3D fromNode, Vector3 startPos, Vector3 endPos)
    {
        return new RaycastBuilder(fromNode).FromPosition(startPos).ToPosition(endPos).Cast();
    }

    public static RaycastHit castRay(
        Node3D fromNode,
        Vector3 startPos,
        Vector3 direction,
        float length,
        RaycastBuilder.DirectionMode directionMode = RaycastBuilder.DirectionMode.Local
    )
    {
        return new RaycastBuilder(fromNode)
            .FromPosition(startPos)
            .WithDirectionMode(directionMode)
            .WithDirectionAndMagnitude(direction, length)
            .Cast();
    }
}
