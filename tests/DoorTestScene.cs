using Godot;

// Headless harness for Door contact behaviour. Builds a door matching the test scene's
// setup, walks a character into it at constant forward input, and prints a per-frame
// trace of the panel angle.
//
//   Godot --path . --headless res://tests/door_test_scene.tscn --max-fps 60 \
//         --quit-after 240 -- <scenario>
//
// Scenarios: opener (default, door flags on), blocked (door flags off)
//
// The scene file carries the stand-in player: PlayerManager scans the tree before this
// node's _Ready, so a player built here would never be registered.
public partial class DoorTestScene : Node3D
{
    private const float CAPSULE_HEIGHT = 2.0F;
    private const float PANEL_HEIGHT = 4.0F;
    private const float PANEL_WIDTH = 2.5F;
    private const float PANEL_THICKNESS = 1.0F;
    private const float START_X = 4.0F;
    private const float STOP_X = -3.0F;

    private DoorTestCharacter _character;
    private WalkingCharacterHandler _handler;
    private RigidBody3D _panel;
    private string _scenario = "opener";
    private int _frame = 0;

    public override void _Ready()
    {
        var userArgs = OS.GetCmdlineUserArgs();
        if (userArgs.Length > 0)
        {
            _scenario = userArgs[0];
        }

        // Ground slab: top face at y = 0.
        addStaticBox(new Vector3(0, -0.5F, 0), new Vector3(20, 1, 20));
        addDoor();
        addCharacter(_scenario == "opener");

        GD.Print(
            $"TEST scenario={_scenario} can_open={_character._CanOpenDoors} "
                + $"can_close={_character._CanCloseDoors}"
        );
    }

    public override void _Process(double delta)
    {
        // Runs before the door's and handler's own _Process: this node is their ancestor.
        var walking = _character.GlobalPosition.X > STOP_X;
        _handler.SetMovementInput(walking ? new Vector2(-1, 0) : Vector2.Zero);
        _frame++;
        GD.Print(
            $"TEST f={_frame} panel_deg={Mathf.RadToDeg(_panel.Rotation.Y):F2} "
                + $"x={_character.GlobalPosition.X:F3} wall={_character.IsOnWall()} "
                + $"panel_contacts={_panel.GetCollidingBodies().Count}"
        );
    }

    // Door panel hinged about world Y at the far edge of the doorway, so the character
    // walks through along X. Mirrors the Door node in scenes/test_scene.tscn.
    private void addDoor()
    {
        var door = new Door();
        door.Name = "Door";
        door.Position = new Vector3(0, (PANEL_HEIGHT / 2F) - 0.5F, 0);

        _panel = new RigidBody3D();
        _panel.Name = "Panel";
        _panel.CollisionLayer = 2;
        _panel.CollisionMask = 2;
        _panel.AxisLockLinearY = true;
        _panel.AxisLockAngularX = true;
        _panel.AxisLockAngularZ = true;
        _panel.ContactMonitor = true;
        _panel.MaxContactsReported = 4;

        var panelShape = new CollisionShape3D();
        panelShape.Shape = new BoxShape3D
        {
            Size = new Vector3(PANEL_THICKNESS, PANEL_HEIGHT, PANEL_WIDTH),
        };
        _panel.AddChild(panelShape);
        _panel.AddChild(
            createInteractionPoint("FrontInteractionPoint", new Vector3(0.125F, -0.5F, -0.8F))
        );
        _panel.AddChild(
            createInteractionPoint("BackInteractionPoint", new Vector3(-0.125F, -0.5F, -0.8F))
        );
        door.AddChild(_panel);

        var hinge = new HingeJoint3D();
        hinge.Name = "Hinge";
        // The hinge axis is the joint's local Z, so rotate it onto world Y.
        hinge.Transform = new Transform3D(
            new Basis(Vector3.Right, Mathf.Pi / 2F),
            new Vector3(0, 0, (PANEL_WIDTH / 2F) + 0.25F)
        );
        hinge.NodeB = "../Panel";
        hinge.SetFlag(HingeJoint3D.Flag.UseLimit, true);
        hinge.SetParam(HingeJoint3D.Param.LimitUpper, 0F);
        hinge.SetParam(HingeJoint3D.Param.LimitLower, 0F);
        door.AddChild(hinge);

        AddChild(door);
    }

    private void addCharacter(bool canUseDoors)
    {
        _character = new DoorTestCharacter();
        _character.Name = "DoorTestCharacter";
        _character._CanOpenDoors = canUseDoors;
        _character._CanCloseDoors = canUseDoors;
        _character.Position = new Vector3(START_X, CAPSULE_HEIGHT / 2F, 0);
        _character.CollisionLayer = 3;
        _character.CollisionMask = 3;

        var collisionShape = new CollisionShape3D();
        collisionShape.Shape = new CapsuleShape3D { Height = CAPSULE_HEIGHT, Radius = 0.5F };
        _character.AddChild(collisionShape);

        var stairProbe = new Node3D();
        stairProbe.Name = "StairProbe";
        stairProbe.Position = new Vector3(0, -CAPSULE_HEIGHT / 2F, 0);
        _character.AddChild(stairProbe);

        AddChild(_character);

        _handler = new WalkingCharacterHandler();
        _handler.Init(_character, new WalkingCharacterSettings(), stairProbe);
        _character.AddChild(_handler);
    }

    private InteractionPoint createInteractionPoint(string name, Vector3 position)
    {
        var interactionPoint = new InteractionPoint();
        interactionPoint.Name = name;
        interactionPoint.Position = position;
        interactionPoint.SetPrompt("Open");
        return interactionPoint;
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
