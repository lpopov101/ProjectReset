using System;
using System.Text;
using Godot;

public partial class TestAnim : Node3D
{
    public override void _Ready()
    {
        var animationName = "Walk";
        var animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        var walkAnimation = animationPlayer.GetAnimation(animationName);
        walkAnimation.LoopMode = Animation.LoopModeEnum.Linear;
        animationPlayer.Play(animationName);
    }
}
