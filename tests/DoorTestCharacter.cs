using Godot;

// Stand-in for a door-using NPC in DoorTestScene. Carries the same flags TestNPC exports
// without pulling in navigation, animation or player chasing.
public partial class DoorTestCharacter : CharacterBody3D, IDoorOpener, IDoorCloser
{
    public bool _CanOpenDoors = false;
    public bool _CanCloseDoors = false;

    public bool CanOpenDoors()
    {
        return _CanOpenDoors;
    }

    public bool CanCloseDoors()
    {
        return _CanCloseDoors;
    }
}
