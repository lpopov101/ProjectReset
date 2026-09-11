using Godot;

// Headless harness for WalkingCharacterHandler stair behaviour.
// Builds known geometry programmatically, walks a character into it at constant
// forward input, and prints a per-frame trace.
//
//   Godot --path . --headless res://tests/stair_test_scene.tscn --max-fps 60 \
//         --quit-after 400 -- <scenario>
//
// Scenarios: climb (default), descend, ledge, jump
public partial class StairTestScene : Node3D
{
    private const float CAPSULE_HEIGHT = 2.0F;
    private const float STEP_HEIGHT = 0.3F;
    private const float STEP_RUN = 0.6F;
    private const int STEP_COUNT = 6;

    private CharacterBody3D _body;
    private WalkingCharacterHandler _handler;
    private string _scenario = "climb";
    private int _frame = 0;

    public override void _Ready()
    {
        var userArgs = OS.GetCmdlineUserArgs();
        if (userArgs.Length > 0)
        {
            _scenario = userArgs[0];
        }

        var startZ = 3.0F;
        var startFootY = 0F;

        // Ground slab: top face at y = 0, spanning z in [-30, 10].
        addStaticBox(new Vector3(0, -0.5F, -10), new Vector3(20, 1, 40));

        if (_scenario == "climb")
        {
            addStaticBox(new Vector3(0, STEP_HEIGHT / 2F, -10), new Vector3(20, STEP_HEIGHT, 20));
        }
        else if (_scenario == "descend")
        {
            // Staircase descending in -z. Character starts on the top landing and
            // walks down. Steps are stacked slabs so each tread is solid below.
            var topY = STEP_HEIGHT * STEP_COUNT;
            addStaticBox(new Vector3(0, topY / 2F, 2.0F), new Vector3(20, topY, 6.0F));
            for (int i = 0; i < STEP_COUNT; i++)
            {
                var treadY = STEP_HEIGHT * (STEP_COUNT - 1 - i);
                var z = -1.0F - (i * STEP_RUN);
                addStaticBox(
                    new Vector3(0, treadY / 2F, z - (STEP_RUN / 2F)),
                    new Vector3(20, Mathf.Max(treadY, 0.02F), STEP_RUN)
                );
            }
            startZ = 3.0F;
            startFootY = topY;
        }
        else if (_scenario == "ledge")
        {
            // Raised platform that simply ends: walking off must produce a real fall.
            addStaticBox(new Vector3(0, 1.5F, 2.0F), new Vector3(20, 3.0F, 8.0F));
            startZ = 4.0F;
            startFootY = 3.0F;
        }

        _body = new CharacterBody3D();
        _body.Name = "TestCharacter";
        _body.Position = new Vector3(0, startFootY + (CAPSULE_HEIGHT / 2F), startZ);

        var collisionShape = new CollisionShape3D();
        collisionShape.Shape = new CapsuleShape3D { Height = CAPSULE_HEIGHT, Radius = 0.5F };
        _body.AddChild(collisionShape);

        var stairProbe = new Node3D();
        stairProbe.Name = "StairProbe";
        stairProbe.Position = new Vector3(0, -CAPSULE_HEIGHT / 2F, 0);
        _body.AddChild(stairProbe);

        AddChild(_body);

        _handler = new WalkingCharacterHandler();
        _handler.Init(_body, new WalkingCharacterSettings(), stairProbe);
        _body.AddChild(_handler);

        GD.Print(
            $"TEST scenario={_scenario} start_foot_y={startFootY:F3} "
                + $"snap_len={_body.FloorSnapLength:F3} step_height={STEP_HEIGHT:F3}"
        );
    }

    public override void _Process(double delta)
    {
        // Runs before the handler's own _Process: this node is the handler's ancestor.
        // Stop driving forward once past the test geometry so low-fps runs (which cover
        // far more ground per frame) stay comparable instead of walking off the slab.
        var walking = _body.GlobalPosition.Z > -8.0F;
        _handler.SetMovementInput(walking ? new Vector2(0, -1) : Vector2.Zero);
        _handler.SetJumpInput(_scenario == "jump" && _frame == 30);
        _frame++;
        GD.Print(
            $"TEST f={_frame} y={_body.GlobalPosition.Y:F4} z={_body.GlobalPosition.Z:F3} "
                + $"vy={_body.Velocity.Y:F3} floor={_body.IsOnFloor()} wall={_body.IsOnWall()}"
        );
    }

    private void addStaticBox(Vector3 position, Vector3 size)
    {
        var staticBody = new StaticBody3D();
        staticBody.Position = position;
        var collisionShape = new CollisionShape3D();
        collisionShape.Shape = new BoxShape3D { Size = size };
        staticBody.AddChild(collisionShape);
        AddChild(staticBody);
    }
}
