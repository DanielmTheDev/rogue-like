namespace RogueLike.Code.Entities.Combat;

/// <summary>
/// Thin combat entry points kept for existing callers. The actual attack behavior
/// (damage, log, kill reaction) lives on <see cref="ICombatant.TryAttack"/>.
/// Scheduled for removal once callers invoke <c>attacker.TryAttack(defender)</c> directly.
/// </summary>
public static class CombatSystem
{
    /// <summary>Resolves an attack when one combatant bumps into another.</summary>
    public static void ResolveBump(ICombatant attacker, ICombatant defender)
        => attacker?.TryAttack(defender);

    /// <summary>Resolves a ranged attack.</summary>
    public static void ResolveRanged(ICombatant attacker, ICombatant defender)
        => attacker?.TryAttack(defender);
}
