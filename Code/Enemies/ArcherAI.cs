using System.Linq;
using Godot;
using RogueLike.Code.Entities;
using RogueLike.Code.Entities.Combat;
using RogueLike.Code.Grid;
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
    private readonly GridMover _mover;
    private readonly int _range;

    public Vector2I GridPosition => _mover.GridPosition;

    public ArcherAI(IActor owner, DungeonGrid grid, EntityManager entityManager, Vector2I startPos, int range = 5)
    {
        _owner = owner;
        _grid = grid;
        _entityManager = entityManager;
        _mover = new GridMover(owner, grid, entityManager, startPos);
        _range = range;
    }

    public void TakeTurn()
    {
        var player = _entityManager.AllActors.FirstOrDefault(a => a.IsPlayer);
        if (player == null)
            return;

        var distance = LineOfSight.ManhattanDistance(_owner.GridPosition, player.GridPosition);
        var hasLos = LineOfSight.HasClearLine(_grid, _owner.GridPosition, player.GridPosition);

        if (hasLos && distance <= _range)
        {
            // Shoot!
            if (_owner is ICombatant attacker && player is ICombatant defender)
                CombatSystem.ResolveRanged(attacker, defender);
            return;
        }

        if (hasLos)
        {
            // Can see the player but out of range — close the distance
            ChasePlayer(player);
            return;
        }

        // No LOS — idle
    }

    private void ChasePlayer(IActor player)
    {
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
                if (targetActor.IsPlayer && targetActor is ICombatant playerCombatant && _owner is ICombatant enemyCombatant)
                    CombatSystem.ResolveBump(enemyCombatant, playerCombatant);
            }
            else
            {
                _mover.TryMove(direction);
            }
        }
    }
}
