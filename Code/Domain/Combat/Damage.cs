using System;

namespace RogueLike.Code.Domain.Combat;

/// <summary>
/// Value object for an amount of combat damage. Immutable, equality-by-value, self-validating:
/// a Damage can never hold a negative amount (invalid states are unconstructable).
/// </summary>
public readonly record struct Damage
{
    public int Amount { get; }

    public Damage(int amount)
    {
        if (amount < 0)
            throw new ArgumentOutOfRangeException(nameof(amount), "Damage cannot be negative.");
        Amount = amount;
    }

    /// <summary>The absence of damage (zero).</summary>
    public static readonly Damage None = new(0);
}
