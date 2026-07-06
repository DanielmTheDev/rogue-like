using Xunit;
using RogueLike.Domain.Actors;
using RogueLike.Domain.Combat;
using RogueLike.Domain.Common;

namespace RogueLike.Domain.Tests.Actors;

/// <summary>
/// Drives the pure <see cref="Actor"/> aggregate: it owns Health + the combat verbs and raises
/// domain events the View renders. Replaces the old default-interface-method <c>ICombatant.TryAttack</c>.
/// </summary>
public class ActorTest
{
    [Fact]
    public void Attack_DealsDamage_AndReturnsTrue()
    {
        var attacker = new TestActor(hp: 10, damage: 3);
        var defender = new DefenderMock(10);

        var hit = attacker.Attack(defender);

        Assert.True(hit);
        Assert.Equal(7, defender.Health.Current);
        Assert.Equal(10, attacker.Health.Current); // attacker takes no damage
    }

    [Fact]
    public void Attack_NullDefender_ReturnsFalse()
    {
        var attacker = new TestActor(hp: 10, damage: 3);

        Assert.False(attacker.Attack(null));
    }

    [Fact]
    public void Attack_KillsDefender_InvokesAttackerOnKilledHook()
    {
        var attacker = new TestActor(hp: 10, damage: 10); // lethal
        var defender = new DefenderMock(5);

        attacker.Attack(defender);

        Assert.True(defender.Health.IsDead);
        Assert.Same(defender, attacker.CapturedVictim);
    }

    [Fact]
    public void Attack_NonLethal_DoesNotInvokeOnKilled()
    {
        var attacker = new TestActor(hp: 10, damage: 2);
        var defender = new DefenderMock(10);

        attacker.Attack(defender);

        Assert.Null(attacker.CapturedVictim);
    }

    [Fact]
    public void ReceiveDamage_RaisesOnHealthChanged()
    {
        var actor = new TestActor(hp: 10, damage: 0);
        (int current, int max) seen = (-1, -1);
        actor.OnHealthChanged += (c, m) => seen = (c, m);

        actor.ReceiveDamage(new Damage(4));

        Assert.Equal((6, 10), seen);
    }

    [Fact]
    public void ReceiveDamage_ToZero_RaisesOnDied()
    {
        var actor = new TestActor(hp: 5, damage: 0);
        var died = false;
        actor.OnDied += () => died = true;

        actor.ReceiveDamage(new Damage(5));

        Assert.True(actor.Health.IsDead);
        Assert.True(died);
    }

    [Fact]
    public void Heal_RestoresHp_AndRaisesOnHealthChanged()
    {
        var actor = new TestActor(hp: 10, damage: 0);
        actor.ReceiveDamage(new Damage(6)); // -> 4/10
        var raised = 0;
        actor.OnHealthChanged += (_, _) => raised++;

        actor.Heal(3); // -> 7/10

        Assert.Equal(7, actor.Health.Current);
        Assert.Equal(1, raised);
    }

    [Fact]
    public void IncreaseMaxHp_RaisesMax_HealsToFull_AndRaisesOnHealthChangedOnce()
    {
        var actor = new TestActor(hp: 10, damage: 0);
        actor.ReceiveDamage(new Damage(4)); // -> 6/10
        (int current, int max) seen = (-1, -1);
        var raised = 0;
        actor.OnHealthChanged += (c, m) => { seen = (c, m); raised++; };

        actor.IncreaseMaxHp(5);

        Assert.Equal((15, 15), seen); // max +5, healed to the new full
        Assert.Equal(1, raised);
    }

    // Concrete Actor for tests: supplies the abstract bits and captures the kill-reaction hook.
    private sealed class TestActor(int hp, int damage) : Actor(hp)
    {
        public override string DisplayName => "Test";
        public override int AttackDamage { get; } = damage;
        public ICombatant CapturedVictim { get; private set; }
        protected override void OnKilled(ICombatant victim) => CapturedVictim = victim;
    }

    // Minimal defender: a plain ICombatant that applies damage to its own Health.
    private sealed class DefenderMock(int hp) : ICombatant
    {
        public string DisplayName => "Defender";
        public Health Health { get; private set; } = new(hp, hp);
        public int AttackDamage => 0;
        public GridPos GridPosition => GridPos.Origin;
        public bool IsPlayer => false;
        public void Die() { }
        public void ReceiveDamage(Damage damage) => Health = Health.TakeDamage(damage.Amount);
        public void Heal(int amount) => Health = Health.Heal(amount);
        public bool Attack(ICombatant defender) => false;
    }
}
