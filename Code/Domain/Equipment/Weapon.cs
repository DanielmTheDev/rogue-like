using System;

namespace RogueLike.Domain.Equipment;

/// <summary>
/// Value object for an equippable weapon. Immutable, equality-by-value,
/// self-validating: a Weapon always has a non-empty name and a damage bonus of
/// at least 1 (invalid states are unconstructable).
/// </summary>
public readonly record struct Weapon
{
    public string Name { get; }
    public int DamageBonus { get; }

    public Weapon(string name, int damageBonus)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Weapon name must not be empty.", nameof(name));
        if (damageBonus < 1)
            throw new ArgumentOutOfRangeException(nameof(damageBonus), "Damage bonus must be at least 1.");
        Name = name;
        DamageBonus = damageBonus;
    }
}
