using System;
using RogueLike.Domain.Combat;
using RogueLike.Domain.Equipment;

namespace RogueLike.Domain.Actors;

/// <summary>
/// The player aggregate: an <see cref="Actor"/> that owns its equipped <see cref="Loadout"/> (so its
/// attack includes the weapon bonus) and reacts to its own kills. The Godot PlayerController is a thin
/// View over this. XP ownership folds in here in Phase 3 batch 2.
/// </summary>
public sealed class Player : Actor
{
    private int _baseAttack;

    public Loadout Loadout { get; } = new();

    public override string DisplayName => "Player";
    public override int AttackDamage => _baseAttack + Loadout.DamageBonus;

    // TRANSITIONAL (DDD Phase 3): the kill reaction is notified out to the PlayerController, which still
    // owns ExperienceSystem. Phase 3 batch 2 moves ExperienceSystem onto this aggregate; OnKilled then
    // awards XP directly and this event (plus the controller's handler) is removed.
    public event Action<ICombatant> OnKilledCombatant;

    public Player(int maxHealth, int baseAttack) : base(maxHealth) => _baseAttack = baseAttack;

    /// <summary>Level-up bump to the base attack (the Loadout bonus is applied on top).</summary>
    public void IncreaseAttack() => _baseAttack++;

    /// <summary>Restore the player to a fresh-run state: base attack, empty loadout, full health.</summary>
    public void ResetForNewGame(int maxHealth, int baseAttack)
    {
        _baseAttack = baseAttack;
        Loadout.Clear();
        ResetHealth(maxHealth);
    }

    protected override void OnKilled(ICombatant victim) => OnKilledCombatant?.Invoke(victim);
}
