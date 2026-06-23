using Xunit;
using RogueLike.Domain.Common;
using RogueLike.Domain.Combat;

namespace RogueLike.Domain.Tests.Combat;

public class CombatantTest
{
    [Fact]
    public void TryAttack_DealsDamage_AndReturnsTrue()
    {
        ICombatant attacker = new MockCombatant(10, 3);
        var defender = new MockCombatant(10, 2);

        var hit = attacker.TryAttack(defender);

        Assert.True(hit);
        Assert.Equal(7, defender.Health.Current);
        Assert.Equal(10, ((MockCombatant)attacker).Health.Current); // attacker takes no damage
    }

    [Fact]
    public void TryAttack_NullDefender_ReturnsFalse()
    {
        ICombatant attacker = new MockCombatant(10, 3);

        Assert.False(attacker.TryAttack(null));
    }

    [Fact]
    public void TryAttack_KillsDefender_TriggersDie()
    {
        ICombatant attacker = new MockCombatant(10, 10);
        var defender = new MockCombatant(5, 0);

        attacker.TryAttack(defender);

        Assert.Equal(0, defender.Health.Current);
        Assert.True(defender.IsDead);
    }

    [Fact]
    public void TryAttack_OnKill_InvokesKillerOnKilledHook()
    {
        ICombatant attacker = new MockCombatant(10, 10); // lethal
        var defender = new MockCombatant(5, 0) { XpReward = 7 };

        attacker.TryAttack(defender);

        Assert.Equal(7, ((MockCombatant)attacker).CapturedKillXp);
    }

    [Fact]
    public void TryAttack_NonLethal_DoesNotInvokeOnKilled()
    {
        ICombatant attacker = new MockCombatant(10, 2);
        var defender = new MockCombatant(10, 0) { XpReward = 7 };

        attacker.TryAttack(defender);

        Assert.Equal(0, ((MockCombatant)attacker).CapturedKillXp);
    }

    private class MockCombatant : ICombatant
    {
        public string DisplayName { get; set; } = "Mock";
        public Health Health { get; private set; }
        public int AttackDamage { get; private set; }
        public int XpReward { get; set; } = 10; // Default XP for mocks
        public GridPos GridPosition { get; set; }
        public bool IsPlayer { get; set; }

        public bool IsDead { get; private set; } = false;

        // Records XP from kills made by this combatant, via the OnKilled hook.
        public int CapturedKillXp { get; private set; }

        public MockCombatant(int hp, int damage)
        {
            Health = new Health(hp, hp);
            AttackDamage = damage;
        }

        public void ReceiveDamage(Damage damage)
        {
            Health = Health.TakeDamage(damage.Amount);
            if (Health.IsDead) Die();
        }

        public void Heal(int amount) => Health = Health.Heal(amount);

        public void Die()
        {
            IsDead = true;
        }

        public void OnKilled(ICombatant victim)
        {
            CapturedKillXp += victim.XpReward;
        }
    }
}
