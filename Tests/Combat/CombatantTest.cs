using GdUnit4;
using RogueLike.Code.Domain.Common;
using RogueLike.Code.Domain.Combat;
using static GdUnit4.Assertions;

namespace RogueLike.Tests.Combat;

[TestSuite]
public class CombatantTest
{
    [TestCase]
    public void TryAttack_DealsDamage_AndReturnsTrue()
    {
        ICombatant attacker = new MockCombatant(10, 3);
        var defender = new MockCombatant(10, 2);

        var hit = attacker.TryAttack(defender);

        AssertBool(hit).IsTrue();
        AssertInt(defender.Health.Current).IsEqual(7);
        AssertInt(((MockCombatant)attacker).Health.Current).IsEqual(10); // attacker takes no damage
    }

    [TestCase]
    public void TryAttack_NullDefender_ReturnsFalse()
    {
        ICombatant attacker = new MockCombatant(10, 3);

        AssertBool(attacker.TryAttack(null)).IsFalse();
    }

    [TestCase]
    public void TryAttack_KillsDefender_TriggersDie()
    {
        ICombatant attacker = new MockCombatant(10, 10);
        var defender = new MockCombatant(5, 0);

        attacker.TryAttack(defender);

        AssertInt(defender.Health.Current).IsEqual(0);
        AssertBool(defender.IsDead).IsTrue();
    }

    [TestCase]
    public void TryAttack_OnKill_InvokesKillerOnKilledHook()
    {
        ICombatant attacker = new MockCombatant(10, 10); // lethal
        var defender = new MockCombatant(5, 0) { XpReward = 7 };

        attacker.TryAttack(defender);

        AssertInt(((MockCombatant)attacker).CapturedKillXp).IsEqual(7);
    }

    [TestCase]
    public void TryAttack_NonLethal_DoesNotInvokeOnKilled()
    {
        ICombatant attacker = new MockCombatant(10, 2);
        var defender = new MockCombatant(10, 0) { XpReward = 7 };

        attacker.TryAttack(defender);

        AssertInt(((MockCombatant)attacker).CapturedKillXp).IsEqual(0);
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
