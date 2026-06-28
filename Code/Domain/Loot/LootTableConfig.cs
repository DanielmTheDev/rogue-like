using System;
using RogueLike.Domain.Common;

namespace RogueLike.Domain.Loot;

/// <summary>
/// Immutable, self-validating tuning for <see cref="WeaponLootTable"/>. Drop chance and
/// per-step upgrade chance both scale with floor depth (clamped by their <c>Max*</c>).
/// A dropped weapon starts at +1 and is upgraded repeatedly while upgrade rolls succeed,
/// up to <see cref="MaxTier"/>. Per-value [0,1] validation is delegated to
/// <see cref="Probability"/>; this type only guards the cross-field invariants. Authored
/// in the view's LevelSettings and translated here so the domain stays Godot-free.
/// </summary>
public readonly record struct LootTableConfig
{
    public Probability BaseDropChance { get; }
    public Probability DropChancePerFloor { get; }
    public Probability MaxDropChance { get; }
    public Probability UpgradeBaseChance { get; }
    public Probability UpgradePerFloor { get; }
    public Probability MaxUpgradeChance { get; }
    public int MaxTier { get; }

    public LootTableConfig(
        Probability baseDropChance, Probability dropChancePerFloor, Probability maxDropChance,
        Probability upgradeBaseChance, Probability upgradePerFloor, Probability maxUpgradeChance,
        int maxTier)
    {
        if (maxDropChance < baseDropChance)
            throw new ArgumentException("MaxDropChance must be >= BaseDropChance.", nameof(maxDropChance));
        if (maxUpgradeChance < upgradeBaseChance)
            throw new ArgumentException("MaxUpgradeChance must be >= UpgradeBaseChance.", nameof(maxUpgradeChance));
        if (maxTier < 1)
            throw new ArgumentOutOfRangeException(nameof(maxTier), maxTier, "MaxTier must be >= 1.");

        BaseDropChance = baseDropChance;
        DropChancePerFloor = dropChancePerFloor;
        MaxDropChance = maxDropChance;
        UpgradeBaseChance = upgradeBaseChance;
        UpgradePerFloor = upgradePerFloor;
        MaxUpgradeChance = maxUpgradeChance;
        MaxTier = maxTier;
    }

    /// <summary>Balanced defaults: deeper floors drop more, and skew to higher tiers.</summary>
    public static LootTableConfig Default => new(
        new Probability(0.25), new Probability(0.05), new Probability(0.75),
        new Probability(0.30), new Probability(0.05), new Probability(0.85),
        5);
}
