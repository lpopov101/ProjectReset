using System;
using Godot;

public partial class SimpleOrbitCamera : Camera3D
{
    [Export]
    private Vector3 _OrbitPoint = Vector3.Zero;

    [Export]
    private float _OrbitDistance = 3F;

    [Export]
    private float _RotateSpeed = 45F;

    private float _verticalAngle = 0;
    private float _horizontalAngle = 0;

    public override void _Ready()
    {
        base._Ready();
        updateTransform();
    }

    public void Rotate(Vector2 direction, double delta)
    {
        _horizontalAngle += direction.X * _RotateSpeed * (float)delta;
        _horizontalAngle = normalizeAngle(_horizontalAngle);

        _verticalAngle -= direction.Y * _RotateSpeed * (float)delta;
        _verticalAngle = Mathf.Clamp(_verticalAngle, -90F, 90F);

        updateTransform();
    }

    public void Reset()
    {
        _horizontalAngle = 0;
        _verticalAngle = 0;
        updateTransform();
    }

    private void updateTransform()
    {
        var radH = Mathf.DegToRad(_horizontalAngle);
        var radV = Mathf.DegToRad(_verticalAngle);
        GlobalPosition =
            _OrbitPoint
            + (
                new Vector3(
                    Mathf.Sin(radH) * Mathf.Cos(radV),
                    Mathf.Sin(radV),
                    Mathf.Cos(radH) * Mathf.Cos(radV)
                ) * _OrbitDistance
            );
        _verticalAngle = Mathf.Clamp(_verticalAngle, -80F, 80F);
        LookAt(_OrbitPoint, Vector3.Up);
    }

    private float normalizeAngle(float angle)
    {
        var result = angle;
        while (result < 0F)
        {
            result += 360F;
        }
        while (result > 360F)
        {
            result -= 360F;
        }
        return result;
    }
}
