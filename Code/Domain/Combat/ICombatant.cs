using RogueLike.Domain.Actors;

namespace RogueLike.Domain.Combat;

/// <summary>
/// A dynamic entity that has a physical presence on the board and can participate in combat.
/// The combat behavior itself lives on the pure <see cref="Actor"/> aggregate; this interface is the
/// polymorphic contract the registry/AI use to reach an actor (as attacker or defender).
/// </summary>
public interface ICombatant : IActor
{
    string DisplayName { get; }
    Health Health { get; }
    int AttackDamage { get; }

    /// <summary>
    /// Invoked when the entity dies, allowing the logic layer to instruct the engine to erase the node.
    /// </summary>
    void Die();

    /// <summary>Apply <paramref name="damage"/> to this combatant's own health, raising its change/death events.</summary>
    void ReceiveDamage(Damage damage);

    /// <summary>Restore HP on this combatant (clamped to Max; the dead cannot be healed).</summary>
    void Heal(int amount);

    /// <summary>
    /// This combatant attacks the defender (deals its damage, logs, fires its own kill reaction on
    /// death). Returns false if there is no defender. The real behavior lives on <see cref="Actor"/>;
    /// implementers delegate to their owned actor.
    /// </summary>
    bool Attack(ICombatant defender);
}
