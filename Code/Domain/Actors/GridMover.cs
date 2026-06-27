using RogueLike.Domain.Common;
using RogueLike.Domain.Grid;

namespace RogueLike.Domain.Actors;

/// <summary>
/// Pure (Godot-free) movement logic for a grid-based entity. Holds the actor's grid position and
/// validates each step against terrain (<see cref="DungeonGrid.CanStep"/>) + occupancy
/// (<see cref="ActorRegistry"/>). World-space pixel projection is a view concern — the view reads
/// <see cref="GridPosition"/> and converts via <c>GridConversions.ToWorldCenter</c>.
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
    /// Access to the underlying grid for walkability checks.
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

        // Terrain traversability (target walkable) is the grid's rule.
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
