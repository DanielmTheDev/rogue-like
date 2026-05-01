using Godot;

namespace RogueLike.Code.World;

/// <summary>
/// A marker class to identify the stairs node in the scene.
/// Allows the PlayerController to detect when it's trying to move onto the stairs.
/// </summary>
public partial class StairsController : Node2D
{
    public Vector2I GridPosition { get; private set; }

    public void Initialize(Vector2I gridPosition)
    {
        GridPosition = gridPosition;
    }
}
