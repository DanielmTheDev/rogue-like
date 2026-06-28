using System;

namespace RogueLike.Domain.Common;

/// <summary>
/// Default <see cref="IRng"/> backed by <see cref="Random"/>. A null seed
/// yields a non-deterministic generator; a value seeds it for reproducible runs.
/// </summary>
public sealed class SystemRng : IRng
{
    private readonly Random _random;

    public SystemRng(int? seed = null)
    {
        _random = seed is { } s ? new Random(s) : new Random();
    }

    public double NextDouble() => _random.NextDouble();

    public int Next(int minInclusive, int maxExclusive) => _random.Next(minInclusive, maxExclusive);
}
