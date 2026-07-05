using RogueLike.Domain.Combat;

namespace RogueLike.Domain.Actors;

/// <summary>
/// A hostile combatant that grants experience when killed. Enemy-only concept — the Player is an
/// <see cref="ICombatant"/> but NOT an <see cref="IEnemy"/>, so it never carries a (meaningless)
/// reward. The kill hook narrows to this interface: `if (victim is IEnemy e) gainXp(e.XpReward)`.
/// </summary>
public interface IEnemy : ICombatant
{
    /// <summary>Experience granted to the killer when this enemy dies.</summary>
    int XpReward { get; }
}
