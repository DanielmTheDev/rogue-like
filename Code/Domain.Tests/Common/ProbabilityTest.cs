using System;
using Xunit;
using RogueLike.Domain.Common;

namespace RogueLike.Domain.Tests.Common;

public class ProbabilityTest
{
    [Theory]
    [InlineData(0.0)]
    [InlineData(0.5)]
    [InlineData(1.0)]
    public void Constructs_WithinUnitInterval(double value)
    {
        var p = new Probability(value);

        Assert.Equal(value, p.Value);
    }

    [Theory]
    [InlineData(-0.01)]
    [InlineData(1.01)]
    public void OutsideUnitInterval_Throws(double value)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Probability(value));
    }

    [Fact]
    public void ImplicitlyConvertsToDouble()
    {
        double d = new Probability(0.3);

        Assert.Equal(0.3, d);
    }

    [Fact]
    public void Equality_IsByValue()
    {
        Assert.True(new Probability(0.4) == new Probability(0.4));
        Assert.False(new Probability(0.4) == new Probability(0.5));
    }
}
