using RogueLike.Domain.Combat;
using RogueLike.Domain.Equipment;
using RogueLike.Domain.Flow;

namespace RogueLike.Domain.Actors;

/// <summary>
/// The player aggregate: an <see cref="Actor"/> that owns its equipped <see cref="Loadout"/> (so its
/// attack includes the weapon bonus), owns its <see cref="ExperienceSystem"/>, and reacts to its own
/// kills by awarding XP. Its level-up reaction raises attack and max HP. The Godot PlayerController is
/// a thin View over this.
/// </summary>
public sealed class Player : Actor
{
    private int _baseAttack;

    public Loadout Loadout { get; } = new();

    /// <summary>The player's XP/level progression. A stable instance that survives a new-game reset.</summary>
    public ExperienceSystem Experience { get; } = new();

    public override string DisplayName => "Player";
    public override int AttackDamage => _baseAttack + Loadout.DamageBonus;

    public Player(int maxHealth, int baseAttack) : base(maxHealth)
    {
        _baseAttack = baseAttack;
        Experience.OnLevelUp += OnLevelUp;
    }

    /// <summary>Level-up bump to the base attack (the Loadout bonus is applied on top).</summary>
    public void IncreaseAttack() => _baseAttack++;

    /// <summary>Restore the player to a fresh-run state: base attack, empty loadout, full health, level 1.</summary>
    public void ResetForNewGame(int maxHealth, int baseAttack)
    {
        _baseAttack = baseAttack;
        Loadout.Clear();
        ResetHealth(maxHealth);
        Experience.Reset();
    }

    /// <summary>Killing an enemy awards its XP reward; a non-enemy combatant grants nothing.</summary>
    protected override void OnKilled(ICombatant victim)
    {
        if (victim is not IEnemy enemy) return;
        Experience.AddXP(enemy.XpReward);
        GameLog.Instance.Log($"[color=yellow]You gained {enemy.XpReward} XP![/color]");
    }

    /// <summary>Reaction to leveling up: raise attack and max HP (healing to the new full), and log it.</summary>
    private void OnLevelUp(int newLevel)
    {
        IncreaseAttack();
        IncreaseMaxHp(5);
        GameLog.Instance.Log($"[color=purple]You reached Level {newLevel}![/color]");
        GameLog.Instance.Log("[color=green]Your Max HP and Attack Damage increase![/color]");
    }
}
