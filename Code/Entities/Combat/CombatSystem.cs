using Godot;

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
        
        // Eventually we might add attack types, combat logs, etc.
    }
}
