using System;
using Xunit;
using RogueLike.Domain.Combat;

namespace RogueLike.Domain.Tests.Combat;

public class DamageTest
{
    [Fact]
    public void Constructor_StoresAmount()
    {
        var damage = new Damage(7);

        Assert.Equal(7, damage.Amount);
    }

    [Fact]
    public void Constructor_NegativeAmount_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Damage(-1));
    }

    [Fact]
    public void None_IsZero()
    {
        Assert.Equal(0, Damage.None.Amount);
    }

    [Fact]
    public void Equality_IsByValue()
    {
        Assert.True(new Damage(5) == new Damage(5));
        Assert.False(new Damage(5) == new Damage(6));
    }
}
