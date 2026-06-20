using RogueLike.Code.Domain.Combat;
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
    // TRANSITIONAL (DDD Phase 3): implemented as a default interface method so it's shared and
    // unit-testable without Godot while controllers still ARE the combatants. Target: a real
    // Attack() method on the pure Actor aggregate (no DIM, no interface-cast at call sites).
    bool TryAttack(ICombatant defender)
    {
        if (defender == null) return false;
        var damage = new Damage(AttackDamage);
        // TRANSITIONAL (DDD Phase 2.5): unwrap to int because HealthController.TakeDamage still
        // takes int; becomes Health.Take(Damage) when Health is promoted to a value object.
        defender.Health.TakeDamage(damage.Amount);
        GameLog.Instance.LogCombat(DisplayName, defender.DisplayName, damage.Amount);
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
