namespace RogueLike.Domain.Actors;

/// <summary>
/// A hostile <see cref="Actor"/>: owns health + a fixed attack. The enemy's decision-making and grid
/// position still live in <c>EnemyAIBase</c> + its <c>GridMover</c> during migration; Phase 3.3 folds
/// those in here so the Enemy aggregate owns position and its own <c>DecideAndAct</c>.
/// </summary>
public class Enemy : Actor
{
    public override string DisplayName { get; }
    public override int AttackDamage { get; }

    public Enemy(string displayName, int maxHealth, int attackDamage) : base(maxHealth)
    {
        DisplayName = displayName;
        AttackDamage = attackDamage;
    }
}
