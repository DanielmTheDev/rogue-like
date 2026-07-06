using RogueLike.Domain.Combat;
using RogueLike.Domain.Equipment;
using RogueLike.Domain.Flow;
using RogueLike.Domain.Items;

namespace RogueLike.Domain.Actors;

/// <summary>
/// The player aggregate: an <see cref="Actor"/> that owns its equipped <see cref="Loadout"/> (so its
/// attack includes the weapon bonus), its <see cref="ExperienceSystem"/> and its <see cref="Inventory"/>,
/// reacts to its own kills by awarding XP, and owns the pickup acquire (<see cref="TryPickup"/>). Its
/// level-up reaction raises attack and max HP. The Godot PlayerController is a thin View over this.
/// </summary>
public sealed class Player : Actor, IItemPicker
{
    private int _baseAttack;

    public Loadout Loadout { get; } = new();

    /// <summary>The player's carried items. A stable instance that survives a new-game reset.</summary>
    public Inventory Inventory { get; } = new(maxSlots: 10);

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

    /// <summary>Restore the player to a fresh-run state: base attack, empty loadout+inventory, full health, level 1.</summary>
    public void ResetForNewGame(int maxHealth, int baseAttack)
    {
        _baseAttack = baseAttack;
        Loadout.Clear();
        Inventory.Clear();
        ResetHealth(maxHealth);
        Experience.Reset();
    }

    /// <summary>
    /// Acquire a floor item: an <see cref="IEquippable"/> equips onto the Loadout (only if it beats the
    /// current weapon), anything else is stored in the Inventory. Returns true if taken (the caller then
    /// unregisters it from the floor), false to leave it. The item never touches the player's aggregates.
    /// </summary>
    public bool TryPickup(IItem item)
    {
        if (!item.CanPickup()) return false;

        if (item is IEquippable equippable)
        {
            if (!Loadout.TryEquip(equippable.Weapon)) return false;
        }
        else if (!Inventory.AddItem(item))
        {
            GameLog.Instance.Log("[color=orange]Your inventory is full![/color]");
            return false;
        }

        item.OnPickup();
        return true;
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
