using RogueLike.Domain.Common;
using RogueLike.Domain.Combat;
using RogueLike.Domain.Grid;

namespace RogueLike.Domain.Actors;

/// <summary>
/// Melee enemy (Goblin): chases a visible player along the shortest path and attacks on contact.
/// Pursuit of a lost player and last-known-position memory live in <see cref="EnemyAIBase"/>.
/// </summary>
public class EnemyAI : EnemyAIBase
{
    public EnemyAI(IActor owner, DungeonGrid grid, ActorRegistry actorRegistry, GridPos startPos, int sightRange)
        : base(owner, grid, actorRegistry, startPos, sightRange)
    {
    }

    protected override void ActOnVisible(IActor player)
    {
        var path = Grid.FindPath(Mover.GridPosition, player.GridPosition);
        if (path is not { Count: > 0 }) return;

        var direction = Mover.GridPosition.DirectionTo(path[0]);
        var next = Mover.GridPosition.Step(direction);
        if (Actors.IsOccupied(next) && Actors.GetActorAt(next) is { IsPlayer: true } and ICombatant defender && Owner is ICombatant attacker)
            attacker.Attack(defender);
        else
            Mover.TryMove(direction);
    }
}
