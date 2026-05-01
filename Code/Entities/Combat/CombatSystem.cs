using Godot;
using RogueLike.Code.Player;
using RogueLike.Code.Services;
using RogueLike.Code.Systems;

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
        
        // Subscribe to the defender's death event to grant XP
        void OnDefenderDied()
        {
            if (attacker is PlayerController player)
            {
                player.Experience.AddXP(defender.XpReward);
                GameLog.Instance.Log($"[color=yellow]You gained {defender.XpReward} XP![/color]");
            }
            // Unsubscribe to prevent memory leaks
            defender.Health.OnDied -= OnDefenderDied;
        }

        defender.Health.OnDied += OnDefenderDied;

        // Very simple logic: attacker deals their direct damage to the defender.
        defender.Health.TakeDamage(attacker.AttackDamage);

        // LOG ACTION
        GameLog.Instance.LogCombat(attacker.DisplayName, defender.DisplayName, attacker.AttackDamage);

        // If the defender didn't die, we must unsubscribe to prevent the event handler
        // from being called on a future, unrelated death.
        if (defender.Health.CurrentHp > 0)
        {
            defender.Health.OnDied -= OnDefenderDied;
        }
    }

    /// <summary>
    /// Resolves a ranged attack.
    /// </summary>
    public static void ResolveRanged(ICombatant attacker, ICombatant defender)
    {
        if (attacker == null || defender == null) return;

        void OnDefenderDied()
        {
            if (attacker is PlayerController player)
            {
                player.Experience.AddXP(defender.XpReward);
                GameLog.Instance.Log($"[color=yellow]You gained {defender.XpReward} XP![/color]");
            }
            defender.Health.OnDied -= OnDefenderDied;
        }

        defender.Health.OnDied += OnDefenderDied;
        
        defender.Health.TakeDamage(attacker.AttackDamage);

        GameLog.Instance.LogCombat(attacker.DisplayName, defender.DisplayName, attacker.AttackDamage);

        if (defender.Health.CurrentHp > 0)
        {
            defender.Health.OnDied -= OnDefenderDied;
        }
    }
}
