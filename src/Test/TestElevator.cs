using System;
using Godot;

public partial class TestElevator : Node3D
{
    [Export]
    private Vector3 _Offset = Vector3.Zero;

    [Export]
    private float _Speed = 10F;

    private Vector3 _initPosition;
    private Vector3 _offsetPosition;
    private Vector3 _targetPosition;

    public override void _Ready()
    {
        base._Ready();
        _initPosition = GlobalPosition;
        _offsetPosition = _initPosition + _Offset;
        _targetPosition = _initPosition;
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        var inverseLerp = LinAlgUtil.Vec3InverseLerp(
            GetNextTargetPosition(),
            _targetPosition,
            GlobalPosition
        );
        GlobalPosition = GetNextTargetPosition()
            .Lerp(_targetPosition, inverseLerp + _Speed * (float)delta);
        GlobalPosition = LinAlgUtil.Vec3RectClamp(GlobalPosition, _initPosition, _offsetPosition);
    }

    public void Toggle()
    {
        _targetPosition = GetNextTargetPosition();
    }

    public void SetInitState()
    {
        _targetPosition = _initPosition;
    }

    public void SetOffsetState()
    {
        _targetPosition = _offsetPosition;
    }

    private Vector3 GetNextTargetPosition()
    {
        return _targetPosition.IsEqualApprox(_initPosition) ? _offsetPosition : _initPosition;
    }
}
