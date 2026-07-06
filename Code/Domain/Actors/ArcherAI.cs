using RogueLike.Domain.Common;
using RogueLike.Domain.Combat;
using RogueLike.Domain.Grid;

namespace RogueLike.Domain.Actors;

/// <summary>
/// Ranged enemy (Skeleton Archer): shoots a visible player when there's a clear line within range,
/// otherwise closes the distance. Pursuit of a lost player is movement-only — it never blind-fires
/// (last-known-position memory + the give-up rule live in <see cref="EnemyAIBase"/>).
/// </summary>
public class ArcherAI : EnemyAIBase
{
    private readonly int _range;

    public ArcherAI(IActor owner, DungeonGrid grid, ActorRegistry actorRegistry, GridPos startPos, int range, int sightRange)
        : base(owner, grid, actorRegistry, startPos, sightRange)
        => _range = range;

    protected override void ActOnVisible(IActor player)
    {
        if (CanShoot(player))
            Shoot(player);
        else
            StepTowards(player.GridPosition);
    }

    private bool CanShoot(IActor player)
    {
        var ownerPos = Mover.GridPosition; // canonical position, not the view-synced owner
        var playerPos = player.GridPosition;
        return Grid.HasClearLine(ownerPos, playerPos) && ownerPos.ManhattanTo(playerPos) <= _range;
    }

    private void Shoot(IActor player)
    {
        if (Owner is ICombatant attacker && player is ICombatant defender)
            attacker.Attack(defender);
    }
}
