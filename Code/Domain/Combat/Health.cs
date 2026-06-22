using System;

namespace RogueLike.Code.Domain.Combat;

/// <summary>
/// Immutable health value object: current/max HP with invariants (0 ≤ Current ≤ Max, Max &gt; 0).
/// Operations return a new <see cref="Health"/>; the owning entity holds the field and raises the
/// change/death events. Invalid states are unconstructable (validated in the constructor).
/// </summary>
public readonly record struct Health
{
    public int Current { get; }
    public int Max { get; }

    public bool IsDead => Current == 0;

    public Health(int current, int max)
    {
        if (max <= 0)
            throw new ArgumentOutOfRangeException(nameof(max), "Max HP must be positive.");
        if (current < 0 || current > max)
            throw new ArgumentOutOfRangeException(nameof(current), "Current HP must be within [0, Max].");

        Current = current;
        Max = max;
    }

    /// <summary>Reduce current HP by <paramref name="amount"/> (clamped at 0). Non-positive amounts are a no-op.</summary>
    public Health TakeDamage(int amount)
        => amount <= 0 ? this : new Health(Math.Max(0, Current - amount), Max);

    /// <summary>Restore current HP by <paramref name="amount"/> (clamped at Max). The dead cannot be healed.</summary>
    public Health Heal(int amount)
        => amount <= 0 || IsDead ? this : new Health(Math.Min(Max, Current + amount), Max);

    /// <summary>Raise Max HP by <paramref name="amount"/> and heal to the new full.</summary>
    public Health WithIncreasedMax(int amount)
    {
        if (amount <= 0) return this;
        var newMax = Max + amount;
        return new Health(newMax, newMax);
    }
}
