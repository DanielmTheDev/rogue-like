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
}
