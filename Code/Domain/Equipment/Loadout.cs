using System;

namespace RogueLike.Domain.Equipment;

/// <summary>
/// Aggregate guarding a player's equipped gear. Owns the "is this weapon better?"
/// invariant: a weapon is equipped only when it strictly beats the current one,
/// so callers cannot downgrade or sidegrade the player.
/// </summary>
public sealed class Loadout
{
    public Weapon? EquippedWeapon { get; private set; }

    public int DamageBonus => EquippedWeapon?.DamageBonus ?? 0;

    public event Action OnEquipmentChanged;

    /// <summary>
    /// Equips <paramref name="weapon"/> only if it is strictly better (higher
    /// damage bonus) than the currently equipped one. Returns true if equipped.
    /// </summary>
    public bool TryEquip(Weapon weapon)
    {
        if (weapon.DamageBonus <= DamageBonus)
            return false;

        EquippedWeapon = weapon;
        OnEquipmentChanged?.Invoke();
        return true;
    }

    /// <summary>
    /// Unequips everything (e.g. on a new game) and notifies listeners.
    /// </summary>
    public void Clear()
    {
        EquippedWeapon = null;
        OnEquipmentChanged?.Invoke();
    }
}
