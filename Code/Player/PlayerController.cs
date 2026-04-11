using Godot;
using RogueLike.Code.Grid;
using RogueLike.Code.TurnContext;

namespace RogueLike.Code.Player;

/// <summary>
/// Handles player input and grid-based movement.
/// Delegates movement logic to GridMover (pure C# / testable).
/// </summary>
public partial class PlayerController : Node2D
{
    private GridMover _mover;
    private TurnManager _turnManager;

    /// <summary>
    /// Current position of the player in grid coordinates.
    /// </summary>
    public Vector2I GridPosition => _mover.GridPosition;

    /// <summary>
    /// Initializes the player on the grid at the given starting position.
    /// </summary>
    public void Initialize(DungeonGrid gridMap, TurnManager turnManager, Vector2I startPos)
    {
        _mover = new GridMover(gridMap, startPos);
        _turnManager = turnManager;
        SyncPosition();
    }

    /// <summary>
    /// Attempts to move the player one tile in the given direction.
    /// Returns true if the move was successful, false if blocked.
    /// </summary>
    public bool TryMove(Vector2I direction)
    {
        if (_mover == null || _turnManager == null)
            return false;

        if (!_mover.TryMove(direction))
            return false;

        SyncPosition();
        return true;
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event.IsEcho() || !@event.IsPressed())
            return;

        if (_turnManager.CurrentState != TurnState.Player)
            return;

        var direction = Vector2I.Zero;

        if (@event.IsActionPressed("move_up"))
            direction = Vector2I.Up;
        else if (@event.IsActionPressed("move_down"))
            direction = Vector2I.Down;
        else if (@event.IsActionPressed("move_left"))
            direction = Vector2I.Left;
        else if (@event.IsActionPressed("move_right"))
            direction = Vector2I.Right;

        if (direction == Vector2I.Zero)
            return;

        if (TryMove(direction))
        {
            _turnManager.EndPlayerTurn();
        }
    }

    private void SyncPosition()
    {
        if (_mover != null)
            Position = _mover.WorldPosition;
    }
}
