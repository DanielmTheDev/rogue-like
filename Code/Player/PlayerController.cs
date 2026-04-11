using Godot;
using RogueLike.Grid;

namespace RogueLike.Player;

/// <summary>
/// Handles player input and grid-based movement.
/// Delegates movement logic to GridMover (pure C# / testable).
/// </summary>
public partial class PlayerController : Node2D
{
    private GridMover _mover;

    /// <summary>
    /// Current position of the player in grid coordinates.
    /// </summary>
    public Vector2I GridPosition => _mover.GridPosition;

    /// <summary>
    /// Initializes the player on the grid at the given starting position.
    /// </summary>
    public void Initialize(DungeonGrid gridMap, Vector2I startPos)
    {
        _mover = new GridMover(gridMap, startPos);
        SyncPosition();
    }

    /// <summary>
    /// Attempts to move the player one tile in the given direction.
    /// Returns true if the move was successful, false if blocked.
    /// </summary>
    public bool TryMove(Vector2I direction)
    {
        if (_mover == null)
            return false;

        if (!_mover.TryMove(direction))
            return false;

        SyncPosition();
        return true;
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is not InputEventKey { Pressed: true } keyEvent)
            return;

        var direction = keyEvent.Keycode switch
        {
            Key.Up or Key.W    => Vector2I.Up,
            Key.Down or Key.S  => Vector2I.Down,
            Key.Left or Key.A  => Vector2I.Left,
            Key.Right or Key.D => Vector2I.Right,
            _ => Vector2I.Zero
        };

        if (direction == Vector2I.Zero)
            return;

        TryMove(direction);
    }

    private void SyncPosition()
    {
        if (_mover != null)
            Position = _mover.WorldPosition;
    }
}
