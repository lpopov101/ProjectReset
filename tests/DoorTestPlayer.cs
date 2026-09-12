using Godot;

// Minimal IPlayer for DoorTestScene: Door and InteractionPoint both resolve the player
// through PlayerManager, so the harness needs one in the tree.
public partial class DoorTestPlayer : CharacterBody3D, IPlayer
{
    private Camera3D _camera;

    public Player CreatePlayer()
    {
        if (_camera == null)
        {
            _camera = new Camera3D();
            AddChild(_camera);
        }
        return new Player(this, _camera);
    }
}
