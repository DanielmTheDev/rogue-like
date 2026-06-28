using System;
using RogueLike.Domain.Common;
using RogueLike.Domain.Equipment;

namespace RogueLike.Domain.Loot;

/// <summary>
/// Domain policy that decides each floor's weapon drop. Given the floor depth it returns
/// the dropped <see cref="Weapon"/>, or null for no drop. Deeper floors raise both the
/// drop chance and the per-step upgrade chance, so dropped weapons trend to higher tiers
/// (Sword +1, +2, … up to MaxTier). Its randomness source is an injected collaborator.
/// </summary>
public sealed class WeaponLootTable
{
    private readonly LootTableConfig _config;
    private readonly IRng _rng;

    public WeaponLootTable(LootTableConfig config, IRng rng)
    {
        _config = config;
        _rng = rng;
    }

    public WeaponLootTable(IRng rng) : this(LootTableConfig.Default, rng) { }

    /// <summary>Rolls this floor's drop. Returns null when nothing drops.</summary>
    public Weapon? Roll(int floorLevel)
    {
        if (floorLevel < 1)
            throw new ArgumentOutOfRangeException(nameof(floorLevel), "Floor level must be >= 1.");

        var n = floorLevel - 1;

        var dropChance = Math.Clamp(_config.BaseDropChance + _config.DropChancePerFloor * n, 0.0, _config.MaxDropChance);
        if (_rng.NextDouble() >= dropChance)
            return null;

        var upgradeChance = Math.Clamp(_config.UpgradeBaseChance + _config.UpgradePerFloor * n, 0.0, _config.MaxUpgradeChance);
        var tier = 1;
        while (tier < _config.MaxTier && _rng.NextDouble() < upgradeChance)
            tier++;

        return new Weapon($"Sword +{tier}", tier);
    }
}
