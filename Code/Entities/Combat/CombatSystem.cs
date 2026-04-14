using Godot;
using RogueLike.Code.Services;

namespace RogueLike.Code.Entities.Combat;

/// <summary>
/// Pure C# combat resolution logic.
/// </summary>
public static class CombatSystem
{
    /// <summary>
    /// Resolves an attack when one combatant bumps into another.
    /// </summary>
    public static void ResolveBump(ICombatant attacker, ICombatant defender)
    {
        if (attacker == null || defender == null) return;
        
        // Very simple logic: attacker deals their direct damage to the defender.
        defender.Health.TakeDamage(attacker.AttackDamage);

        // LOG ACTION
        GameLog.Instance.LogCombat(attacker.Name, defender.Name, attacker.AttackDamage);
    }

    /// <summary>
    /// Resolves a ranged attack. Separate from bump to allow future divergence
    /// (e.g., damage falloff, dodge chance, cover bonuses).
    /// </summary>
    public static void ResolveRanged(ICombatant attacker, ICombatant defender)
    {
        if (attacker == null || defender == null) return;
        
        defender.Health.TakeDamage(attacker.AttackDamage);

        // LOG ACTION
        GameLog.Instance.LogCombat(attacker.Name, defender.Name, attacker.AttackDamage);
    }
}
