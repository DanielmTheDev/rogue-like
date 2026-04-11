using Godot;
using RogueLike.Code.Grid;

namespace RogueLike.Code.Player;

/// <summary>
/// Pure movement logic for a grid-based entity.
/// No Godot Node dependency — easily testable.
/// </summary>
public class GridMover
{
    private readonly DungeonGrid _grid;
    private Vector2I _gridPosition;

    /// <summary>
    /// Current position in grid coordinates.
    /// </summary>
    public Vector2I GridPosition => _gridPosition;

    /// <summary>
    /// Current position in world-space pixels (center of tile).
    /// </summary>
    public Vector2 WorldPosition => _grid.GridToWorld(_gridPosition);

    public GridMover(DungeonGrid grid, Vector2I startPos)
    {
        _grid = grid;
        _gridPosition = startPos;
    }

    /// <summary>
    /// Attempts to move one tile in the given direction.
    /// Returns true if the move was successful, false if blocked.
    /// </summary>
    public bool TryMove(Vector2I direction)
    {
        var target = _gridPosition + direction;

        if (!_grid.IsWalkable(target))
            return false;

        _gridPosition = target;
        return true;
    }
}
