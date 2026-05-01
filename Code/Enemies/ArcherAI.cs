using System.Linq;
using Godot;
using RogueLike.Code.Entities;
using RogueLike.Code.Entities.Combat;
using RogueLike.Code.Grid;
using RogueLike.Code.Pathfinding;
using RogueLike.Code.Player;

namespace RogueLike.Code.Enemies;

/// <summary>
/// Pure C# AI for the Skeleton Archer.
/// Behavior: If player is in LOS and within range → shoot.
///           If player is visible but out of range → walk closer.
///           Otherwise → idle.
/// </summary>
public class ArcherAI
{
    private readonly IActor _owner;
    private readonly DungeonGrid _grid;
    private readonly EntityManager _entityManager;
    private readonly Pathfinder _pathfinder;
    private readonly GridMover _mover;
    private readonly int _range;

    public Vector2I GridPosition => _mover.GridPosition;

    public ArcherAI(IActor owner, DungeonGrid grid, EntityManager entityManager, Pathfinder pathfinder, Vector2I startPos, int range = 5)
    {
        _owner = owner;
        _grid = grid;
        _entityManager = entityManager;
        _pathfinder = pathfinder;
        _mover = new GridMover(owner, grid, entityManager, startPos);
        _range = range;
    }

    public void TakeTurn(RogueLike.Code.Grid.FOV.FovMap fovMap)
    {
        var player = _entityManager.AllActors.FirstOrDefault(a => a.IsPlayer);
        if (player == null) return;
        
        bool isPlayerVisible = fovMap.GetVisibility(player.GridPosition) == Code.Grid.FOV.VisibilityState.Visible;
        if (!isPlayerVisible)
        {
            // Player not visible, do nothing
            return;
        }

        var distance = LineOfSight.ManhattanDistance(_owner.GridPosition, player.GridPosition);
        var hasLos = LineOfSight.HasClearLine(_grid, _owner.GridPosition, player.GridPosition);
        
        if (hasLos && distance <= _range)
        {
            // In range and has a clear line of sight, shoot!
            if (_owner is ICombatant attacker && player is ICombatant defender)
                CombatSystem.ResolveRanged(attacker, defender);
        }
        else if (isPlayerVisible)
        {
            // Visible (e.g., around a corner) but out of range or LOS, move closer.
            ChasePlayer(player);
        }
    }

    private void ChasePlayer(IActor player)
    {
        var path = _pathfinder.FindPath(_owner.GridPosition, player.GridPosition, _grid);
        if (path != null && path.Count > 0)
        {
            var direction = path[0] - _owner.GridPosition;
            _mover.TryMove(direction);
        }
    }
}
