using System;

namespace RogueLike.Domain.Common;

/// <summary>
/// Value object for a probability in [0,1]. Immutable, equality-by-value, self-validating:
/// invalid values are unconstructable. Converts implicitly to <see cref="double"/> so it
/// reads naturally in arithmetic.
/// </summary>
public readonly record struct Probability
{
    public double Value { get; }

    public Probability(double value)
    {
        if (value is < 0.0 or > 1.0)
            throw new ArgumentOutOfRangeException(nameof(value), value, "Probability must be in [0,1].");
        Value = value;
    }

    public static implicit operator double(Probability probability) => probability.Value;
}
