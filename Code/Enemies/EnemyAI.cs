using System.Linq;
using Godot;
using RogueLike.Code.Entities;
using RogueLike.Code.Entities.Combat;
using RogueLike.Code.Grid;
using RogueLike.Code.Pathfinding;
using RogueLike.Code.Player;

namespace RogueLike.Code.Enemies;

/// <summary>
/// Pure C# class representing the AI decision making for an Enemy.
/// </summary>
public class EnemyAI
{
    private readonly IActor _owner;
    private readonly DungeonGrid _grid;
    private readonly EntityManager _entityManager;
    private readonly Pathfinder _pathfinder;
    private readonly GridMover _mover;

    /// <summary>
    /// Expose GridMover position to the node.
    /// </summary>
    public Vector2I GridPosition => _mover.GridPosition;

    public EnemyAI(IActor owner, DungeonGrid grid, EntityManager entityManager, Pathfinder pathfinder, Vector2I startPos)
    {
        _owner = owner;
        _grid = grid;
        _entityManager = entityManager;
        _pathfinder = pathfinder;
        _mover = new GridMover(owner, grid, entityManager, startPos);
    }

    /// <summary>
    /// Evaluates game state and makes a single move.
    /// </summary>
    public void TakeTurn(RogueLike.Code.Grid.FOV.FovMap fovMap)
    {
        var player = _entityManager.AllActors.FirstOrDefault(a => a.IsPlayer);
        if (player == null) return;

        // If player is not visible, do nothing for now.
        // Future AI could move towards last known position.
        if (fovMap.GetVisibility(player.GridPosition) != Code.Grid.FOV.VisibilityState.Visible)
        {
            // TODO: Add random wandering later
            return;
        }

        var path = _pathfinder.FindPath(_owner.GridPosition, player.GridPosition, _grid);

        if (path != null && path.Count > 0)
        {
            var nextStep = path[0];
            var direction = nextStep - _owner.GridPosition;

            var target = _owner.GridPosition + direction;
            if (_entityManager.IsOccupied(target))
            {
                var targetActor = _entityManager.GetActorAt(target);
                if (targetActor.IsPlayer && targetActor is ICombatant playerCombatant && _owner is ICombatant enemyCombatant)
                {
                    enemyCombatant.TryAttack(playerCombatant);
                }
            }
            else
            {
                _mover.TryMove(direction);
            }
        }
    }
}
