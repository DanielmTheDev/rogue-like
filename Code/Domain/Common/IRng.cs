namespace RogueLike.Domain.Common;

/// <summary>
/// Randomness seam for the domain. Lets domain logic (e.g. loot rolls) depend on
/// an abstraction rather than concrete <see cref="System.Random"/>, so callers can
/// inject a seeded or scripted generator for deterministic tests.
/// </summary>
public interface IRng
{
    /// <summary>A double in [0.0, 1.0).</summary>
    double NextDouble();

    /// <summary>An int in [minInclusive, maxExclusive).</summary>
    int Next(int minInclusive, int maxExclusive);
}
