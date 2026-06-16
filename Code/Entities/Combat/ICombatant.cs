using RogueLike.Code.Services;

namespace RogueLike.Code.Entities.Combat;

/// <summary>
/// A dynamic entity that has a physical presence on the board and can participate in combat.
/// </summary>
public interface ICombatant : IActor
{
    string DisplayName { get; }
    HealthController Health { get; }
    int AttackDamage { get; }
    int XpReward { get; }

    /// <summary>
    /// Invoked when the entity dies, allowing the logic layer to instruct the engine to erase the node.
    /// </summary>
    void Die();

    /// <summary>
    /// This combatant attacks the defender: deals its damage and logs the action.
    /// Behavior lives with the data it mutates (rich domain). Returns false if there is no defender.
    /// Death/XP consequences are handled by the caller observing <see cref="HealthController.OnDied"/>.
    /// </summary>
    bool TryAttack(ICombatant defender)
    {
        if (defender == null) return false;
        defender.Health.TakeDamage(AttackDamage);
        GameLog.Instance.LogCombat(DisplayName, defender.DisplayName, AttackDamage);
        return true;
    }
}
