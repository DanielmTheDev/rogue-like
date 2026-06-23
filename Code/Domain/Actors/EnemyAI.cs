using System.Linq;
using RogueLike.Code.Domain.Common;
using RogueLike.Code.Domain.Combat;
using RogueLike.Code.Domain.Grid;

namespace RogueLike.Code.Domain.Actors;

/// <summary>
/// Pure C# class representing the AI decision making for an Enemy.
/// </summary>
// TRANSITIONAL (DDD Phase 3.3): the entity is split across EnemyController (node) + this AI +
// the GridMover it owns (which actually holds the enemy's position). Target: a pure
// Enemy : Actor aggregate that owns position/health and its own DecideAndAct(TurnContext);
// EnemyController demotes to a pure View. This separate AI object then disappears.
public class EnemyAI
{
    private readonly IActor _owner;
    private readonly DungeonGrid _grid;
    private readonly ActorRegistry _actorRegistry;
    private readonly GridMover _mover;

    /// <summary>
    /// Expose GridMover position to the node.
    /// </summary>
    public GridPos GridPosition => _mover.GridPosition;

    public EnemyAI(IActor owner, DungeonGrid grid, ActorRegistry actorRegistry, GridPos startPos)
    {
        _owner = owner;
        _grid = grid;
        _actorRegistry = actorRegistry;
        _mover = new GridMover(owner, grid, actorRegistry, startPos);
    }

    /// <summary>
    /// Evaluates game state and makes a single move.
    /// </summary>
    public void TakeTurn(Grid.FOV.FovMap fovMap)
    {
        var player = _actorRegistry.AllActors.FirstOrDefault(a => a.IsPlayer);
        if (player == null) return;

        // If player is not visible, do nothing for now.
        // Future AI could move towards last known position.
        if (fovMap.GetVisibility(player.GridPosition) != Grid.FOV.VisibilityState.Visible)
        {
            // TODO: Add random wandering later
            return;
        }

        var path = _grid.FindPath(_owner.GridPosition, player.GridPosition);

        if (path != null && path.Count > 0)
        {
            var nextStep = path[0];
            var direction = _owner.GridPosition.DirectionTo(nextStep);

            var target = _owner.GridPosition.Step(direction);
            if (_actorRegistry.IsOccupied(target))
            {
                var targetActor = _actorRegistry.GetActorAt(target);
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
