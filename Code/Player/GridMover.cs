using Godot;
using RogueLike.Code.Domain.Common;
using RogueLike.Code.Domain.Grid;
using RogueLike.Code.Domain.Actors;
using RogueLike.Code.View;

namespace RogueLike.Code.Player;

/// <summary>
/// Pure movement logic for a grid-based entity.
/// No Godot Node dependency — easily testable.
/// </summary>
public class GridMover
{
    private readonly DungeonGrid _grid;
    private readonly ActorRegistry _actorRegistry;
    private readonly IActor _owner;
    private GridPos _gridPosition;

    /// <summary>
    /// Current position in grid coordinates.
    /// </summary>
    public GridPos GridPosition => _gridPosition;

    /// <summary>
    /// Current position in world-space pixels (center of tile).
    /// </summary>
    public Vector2 WorldPosition => _gridPosition.ToWorldCenter(_grid.TileSize);

    /// <summary>
    /// Access to the underlying grid for wall/corner checks.
    /// </summary>
    public DungeonGrid Grid => _grid;

    public GridMover(IActor owner, DungeonGrid grid, ActorRegistry actorRegistry, GridPos startPos)
    {
        _owner = owner;
        _grid = grid;
        _actorRegistry = actorRegistry;
        _gridPosition = startPos;
    }

    /// <summary>
    /// Attempts to move one tile in the given direction.
    /// Returns true if the move was successful, false if blocked.
    /// </summary>
    public bool TryMove(Direction direction)
    {
        var target = _gridPosition.Step(direction);

        // Terrain traversability (walkable + no diagonal corner-cut) is the grid's rule.
        if (!_grid.CanStep(_gridPosition, direction))
            return false;

        if (_actorRegistry.IsOccupied(target))
            return false;

        var oldPos = _gridPosition;
        _gridPosition = target;

        _actorRegistry.UpdateActorPosition(_owner, oldPos, _gridPosition);
        return true;
    }
}
