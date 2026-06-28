using System;
using Xunit;
using RogueLike.Domain.Common;
using RogueLike.Domain.Equipment;
using RogueLike.Domain.Loot;
using RogueLike.Domain.Tests.Common;

namespace RogueLike.Domain.Tests.Loot;

// Default config: drop = clamp(0.25 + 0.05n, .., 0.75); upgrade = clamp(0.30 + 0.05n, .., 0.85),
// n = floor-1, MaxTier 5. A drop starts at +1 and upgrades while NextDouble() < upgradeChance.
public class WeaponLootTableTest
{
    [Fact]
    public void Roll_FloorBelowOne_Throws()
    {
        var table = new WeaponLootTable(new ScriptedRng(0.5));

        Assert.Throws<ArgumentOutOfRangeException>(() => { table.Roll(0); });
    }

    [Fact]
    public void Roll_DrawAtOrAboveDropChance_ReturnsNull_AndConsumesOnlyOneDraw()
    {
        // floor 1 drop chance = 0.25; 0.25 is NOT < 0.25 -> no drop. Only one scripted value:
        // a second draw would throw.
        var table = new WeaponLootTable(new ScriptedRng(0.25));

        Assert.Null(table.Roll(1));
    }

    [Fact]
    public void Roll_Drop_FirstUpgradeFails_GivesSwordPlusOne()
    {
        // floor 1: drop 0.20 < 0.25 -> drops; upgrade 0.90 >= 0.30 -> stop at tier 1.
        var table = new WeaponLootTable(new ScriptedRng(0.20, 0.90));

        Assert.Equal(new Weapon("Sword +1", 1), table.Roll(1));
    }

    [Fact]
    public void Roll_Drop_UpgradesEscalateTier()
    {
        // floor 1: drop 0.20 -> drops; 0.10,0.10 < 0.30 -> +2,+3; 0.90 -> stop. Tier 3.
        var table = new WeaponLootTable(new ScriptedRng(0.20, 0.10, 0.10, 0.90));

        Assert.Equal(new Weapon("Sword +3", 3), table.Roll(1));
    }

    [Fact]
    public void Roll_UpgradesCapAtMaxTier()
    {
        // Drop, then 4 successful upgrades reach tier 5 (MaxTier); loop stops without a 5th draw.
        var table = new WeaponLootTable(new ScriptedRng(0.20, 0.0, 0.0, 0.0, 0.0));

        Assert.Equal(new Weapon("Sword +5", 5), table.Roll(1));
    }

    [Fact]
    public void DropChance_RisesWithDepth()
    {
        // Same drop draw 0.70: floor 1 (drop 0.25) no drop; floor 11 (drop 0.75) drops.
        Assert.Null(new WeaponLootTable(new ScriptedRng(0.70)).Roll(1));
        Assert.NotNull(new WeaponLootTable(new ScriptedRng(0.70, 0.99)).Roll(11));
    }

    [Fact]
    public void UpgradeChance_RisesWithDepth()
    {
        // Same upgrade draw 0.50: floor 1 (upgrade 0.30) stops at +1; floor 11 (upgrade 0.80) upgrades.
        Assert.Equal(new Weapon("Sword +1", 1), new WeaponLootTable(new ScriptedRng(0.05, 0.50)).Roll(1));
        Assert.Equal(new Weapon("Sword +2", 2), new WeaponLootTable(new ScriptedRng(0.05, 0.50, 0.90)).Roll(11));
    }

    [Fact]
    public void DropChance_ClampsAtMax_OnVeryDeepFloors()
    {
        // floor 999 drop clamps to 0.75: 0.76 >= 0.75 -> no drop.
        Assert.Null(new WeaponLootTable(new ScriptedRng(0.76)).Roll(999));
        // 0.74 < 0.75 -> drops; upgrade clamps to 0.85: 0.90 >= 0.85 -> stop at +1.
        Assert.Equal(new Weapon("Sword +1", 1), new WeaponLootTable(new ScriptedRng(0.74, 0.90)).Roll(999));
    }

    [Fact]
    public void Roll_WithSystemRng_SameSeed_IsReproducible()
    {
        var a = new WeaponLootTable(new SystemRng(42));
        var b = new WeaponLootTable(new SystemRng(42));

        for (var floor = 1; floor <= 15; floor++)
            Assert.Equal(a.Roll(floor), b.Roll(floor));
    }
}
