using Xunit;
using RogueLike.Domain.Actors;
using RogueLike.Domain.Combat;
using RogueLike.Domain.Common;
using RogueLike.Domain.Equipment;

namespace RogueLike.Domain.Tests.Actors;

/// <summary>
/// The Player aggregate owns its Loadout (so attack includes the weapon bonus) and its
/// <see cref="Player.Experience"/>: killing an <see cref="IEnemy"/> awards XP directly, and its own
/// level-up reaction raises attack and max HP.
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
    public void Attack_KillsEnemy_AwardsXp()
    {
        var player = new Player(maxHealth: 20, baseAttack: 10);
        var enemy = new EnemyMock(hp: 5, xpReward: 35);

        player.Attack(enemy);

        Assert.True(enemy.Health.IsDead);
        Assert.Equal(35, player.Experience.CurrentXP);
    }

    [Fact]
    public void Attack_KillsNonEnemyCombatant_AwardsNoXp()
    {
        var player = new Player(maxHealth: 20, baseAttack: 10);
        var victim = new VictimMock(5);

        player.Attack(victim);

        Assert.True(victim.Health.IsDead);
        Assert.Equal(0, player.Experience.CurrentXP);
    }

    [Fact]
    public void LevelUp_IncreasesAttackAndMaxHp()
    {
        var player = new Player(maxHealth: 20, baseAttack: 10);
        var enemy = new EnemyMock(hp: 5, xpReward: 100); // exactly one level's worth

        player.Attack(enemy);

        Assert.Equal(2, player.Experience.CurrentLevel);
        Assert.Equal(11, player.AttackDamage); // base +1
        Assert.Equal(25, player.Health.Max);   // +5, healed to full
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

    [Fact]
    public void ResetForNewGame_ResetsExperienceToLevel1()
    {
        var player = new Player(maxHealth: 20, baseAttack: 10);
        player.Attack(new EnemyMock(hp: 5, xpReward: 150)); // -> level 2, 50 XP

        player.ResetForNewGame(maxHealth: 20, baseAttack: 10);

        Assert.Equal(1, player.Experience.CurrentLevel);
        Assert.Equal(0, player.Experience.CurrentXP);
        Assert.Equal(100, player.Experience.XPForNextLevel);
    }

    private class VictimMock(int hp) : ICombatant
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

    private sealed class EnemyMock(int hp, int xpReward) : VictimMock(hp), IEnemy
    {
        public int XpReward => xpReward;
    }
}
