using Xunit;
using RogueLike.Domain.Actors;
using RogueLike.Domain.Combat;
using RogueLike.Domain.Common;
using RogueLike.Domain.Equipment;

namespace RogueLike.Domain.Tests.Actors;

/// <summary>
/// The Player aggregate owns its Loadout (so attack includes the weapon bonus) and reacts to its own
/// kills by raising <see cref="Player.OnKilledCombatant"/> (the transitional XP bridge to the View).
/// </summary>
public class PlayerTest
{
    [Fact]
    public void AttackDamage_Unarmed_IsBaseAttack()
    {
        var player = new Player(maxHealth: 20, baseAttack: 3);

        Assert.Equal(3, player.AttackDamage);
    }

    [Fact]
    public void AttackDamage_IncludesEquippedWeaponBonus()
    {
        var player = new Player(maxHealth: 20, baseAttack: 3);

        player.Loadout.TryEquip(new Weapon("Sword +1", 1));

        Assert.Equal(4, player.AttackDamage);
    }

    [Fact]
    public void IncreaseAttack_RaisesBaseAttack()
    {
        var player = new Player(maxHealth: 20, baseAttack: 3);

        player.IncreaseAttack();

        Assert.Equal(4, player.AttackDamage);
    }

    [Fact]
    public void Attack_KillsVictim_RaisesOnKilledCombatant()
    {
        var player = new Player(maxHealth: 20, baseAttack: 10);
        var victim = new VictimMock(5);
        ICombatant killed = null;
        player.OnKilledCombatant += v => killed = v;

        player.Attack(victim);

        Assert.True(victim.Health.IsDead);
        Assert.Same(victim, killed);
    }

    [Fact]
    public void ResetForNewGame_ClearsLoadout_AndRestoresBaseAttackAndHealth()
    {
        var player = new Player(maxHealth: 20, baseAttack: 3);
        player.Loadout.TryEquip(new Weapon("Sword +2", 2));
        player.IncreaseAttack(); // -> base 4
        player.ReceiveDamage(new Damage(10)); // -> 10/20

        player.ResetForNewGame(maxHealth: 20, baseAttack: 3);

        Assert.Null(player.Loadout.EquippedWeapon);
        Assert.Equal(3, player.AttackDamage);
        Assert.Equal(20, player.Health.Current);
    }

    private sealed class VictimMock(int hp) : ICombatant
    {
        public string DisplayName => "Victim";
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
