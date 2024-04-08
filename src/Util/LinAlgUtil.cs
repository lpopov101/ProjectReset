using System;
using Godot;

class LinAlgUtil
{
    public static Transform3D QuickLookAt(Transform3D transform, Vector3 target)
    {
        Vector3 directionToTarget = (target - transform.Origin).Normalized();
        Vector3 transformForward = TransformForward(transform);
        Vector3 up = transformForward.IsEqualApprox(directionToTarget)
            ? TransformUp(transform)
            : transformForward.Cross(directionToTarget);

        return transform.LookingAt(target, up);
    }

    public static float Vec3InverseLerp(Vector3 from, Vector3 to, Vector3 current)
    {
        var sum = 0F;
        var count = 0;
        var inverseLerpX = Mathf.InverseLerp(from.X, to.X, current.X);
        if (Mathf.IsFinite(inverseLerpX))
        {
            sum += inverseLerpX;
            count++;
        }
        var inverseLerpY = Mathf.InverseLerp(from.Y, to.Y, current.Y);
        if (Mathf.IsFinite(inverseLerpY))
        {
            sum += inverseLerpY;
            count++;
        }
        var inverseLerpZ = Mathf.InverseLerp(from.Z, to.Z, current.Z);
        if (Mathf.IsFinite(inverseLerpZ))
        {
            sum += inverseLerpZ;
            count++;
        }
        return count == 0 ? 0 : sum / (float)count;
    }

    public static Vector3 Vec3RectClamp(Vector3 current, Vector3 from, Vector3 to)
    {
        var minVec = new Vector3(
            Mathf.Min(from.X, to.X),
            Mathf.Min(from.Y, to.Y),
            Mathf.Min(from.Z, to.Z)
        );
        var maxVec = new Vector3(
            Mathf.Max(from.X, to.X),
            Mathf.Max(from.Y, to.Y),
            Mathf.Max(from.Z, to.Z)
        );
        return new Vector3(
            Mathf.Clamp(current.X, minVec.X, maxVec.X),
            Mathf.Clamp(current.Y, minVec.Y, maxVec.Y),
            Mathf.Clamp(current.Z, minVec.Z, maxVec.Z)
        );
    }

    public static Vector3 TransformUp(Transform3D transform)
    {
        return transform.Basis.Y;
    }

    public static Vector3 TransformDown(Transform3D transform)
    {
        return -transform.Basis.Y;
    }

    public static Vector3 TransformRight(Transform3D transform)
    {
        return transform.Basis.X;
    }

    public static Vector3 TransformLeft(Transform3D transform)
    {
        return -transform.Basis.X;
    }

    public static Vector3 TransformForward(Transform3D transform)
    {
        return -transform.Basis.Z;
    }

    public static Vector3 TransformBackward(Transform3D transform)
    {
        return transform.Basis.Z;
    }
}
