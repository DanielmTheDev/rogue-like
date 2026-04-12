using System.Linq;
using Godot;
using RogueLike.Code.Entities;
using RogueLike.Code.Entities.Combat;
using RogueLike.Code.Grid;
using RogueLike.Code.Player;

namespace RogueLike.Code.Enemies;

/// <summary>
/// Pure C# class representing the AI decision making for an Enemy.
/// </summary>
public class EnemyAI
{
    private readonly IActor _owner;
    private readonly EntityManager _entityManager;
    private readonly GridMover _mover;

    /// <summary>
    /// Expose GridMover position to the node.
    /// </summary>
    public Vector2I GridPosition => _mover.GridPosition;

    public EnemyAI(IActor owner, DungeonGrid grid, EntityManager entityManager, Vector2I startPos)
    {
        _owner = owner;
        _entityManager = entityManager;
        _mover = new GridMover(owner, grid, entityManager, startPos);
    }

    /// <summary>
    /// Evaluates game state and makes a single move.
    /// </summary>
    public void TakeTurn()
    {
        var player = _entityManager.AllActors.FirstOrDefault(a => a.IsPlayer);
        if (player == null)
            return;

        // Extremely simple AI: Step towards the player (X first, then Y)
        var toPlayer = player.GridPosition - _owner.GridPosition;
        var direction = Vector2I.Zero;

        if (toPlayer.X > 0) direction.X = 1;
        else if (toPlayer.X < 0) direction.X = -1;
        else if (toPlayer.Y > 0) direction.Y = 1;
        else if (toPlayer.Y < 0) direction.Y = -1;

        if (direction != Vector2I.Zero)
        {
            var target = _owner.GridPosition + direction;
            if (_entityManager.IsOccupied(target))
            {
                var targetActor = _entityManager.GetActorAt(target);
                // Only attack the player, prevent goblins fighting goblins
                if (targetActor.IsPlayer && targetActor is ICombatant playerCombatant && _owner is ICombatant enemyCombatant)
                {
                    CombatSystem.ResolveBump(enemyCombatant, playerCombatant);
                }
            }
            else
            {
                _mover.TryMove(direction);
            }
        }
    }
}
