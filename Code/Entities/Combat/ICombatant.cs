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
    /// This combatant attacks the defender: deals its damage, logs the action, and—if the
    /// defender dies—invokes <see cref="OnKilled"/> on this attacker. Behavior lives with the
    /// data it mutates (rich domain). Returns false if there is no defender.
    /// </summary>
    bool TryAttack(ICombatant defender)
    {
        if (defender == null) return false;
        defender.Health.TakeDamage(AttackDamage);
        GameLog.Instance.LogCombat(DisplayName, defender.DisplayName, AttackDamage);
        if (defender.Health.CurrentHp <= 0) OnKilled(defender);
        return true;
    }

    /// <summary>
    /// Reaction hook invoked on this combatant when its attack kills <paramref name="victim"/>.
    /// Default: no reaction. The player overrides this to gain XP — combat no longer needs to
    /// know who the attacker is (replaces the old `attacker is PlayerController` cast).
    /// </summary>
    void OnKilled(ICombatant victim) { }
}
