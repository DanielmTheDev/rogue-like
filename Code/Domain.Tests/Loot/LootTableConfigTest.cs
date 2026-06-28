using System;
using Xunit;
using RogueLike.Domain.Common;
using RogueLike.Domain.Loot;

namespace RogueLike.Domain.Tests.Loot;

public class LootTableConfigTest
{
    [Fact]
    public void Default_Constructs()
    {
        var config = LootTableConfig.Default;

        Assert.True(config.BaseDropChance.Value > 0);
        Assert.True(config.MaxDropChance >= config.BaseDropChance);
        Assert.True(config.MaxTier >= 1);
    }

    [Fact]
    public void MaxDropBelowBase_Throws()
    {
        // maxDrop 0.3 < base 0.5
        Assert.Throws<ArgumentException>(() => new LootTableConfig(
            new Probability(0.5), new Probability(0.05), new Probability(0.3),
            new Probability(0.30), new Probability(0.05), new Probability(0.85), 5));
    }

    [Fact]
    public void MaxUpgradeBelowBase_Throws()
    {
        // maxUpgrade 0.3 < base 0.5
        Assert.Throws<ArgumentException>(() => new LootTableConfig(
            new Probability(0.25), new Probability(0.05), new Probability(0.75),
            new Probability(0.5), new Probability(0.05), new Probability(0.3), 5));
    }

    [Fact]
    public void MaxTierBelowOne_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new LootTableConfig(
            new Probability(0.25), new Probability(0.05), new Probability(0.75),
            new Probability(0.30), new Probability(0.05), new Probability(0.85), 0));
    }
}
