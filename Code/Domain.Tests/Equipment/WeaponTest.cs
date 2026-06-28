using System;
using Xunit;
using RogueLike.Domain.Equipment;

namespace RogueLike.Domain.Tests.Equipment;

public class WeaponTest
{
    [Fact]
    public void Constructor_StoresNameAndDamageBonus()
    {
        var weapon = new Weapon("Sword +1", 1);

        Assert.Equal("Sword +1", weapon.Name);
        Assert.Equal(1, weapon.DamageBonus);
    }

    [Fact]
    public void Constructor_EmptyName_Throws()
    {
        Assert.Throws<ArgumentException>(() => new Weapon("  ", 1));
    }

    [Fact]
    public void Constructor_DamageBonusBelowOne_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Weapon("Stick", 0));
    }

    [Fact]
    public void Equality_IsByValue()
    {
        Assert.True(new Weapon("Sword +1", 1) == new Weapon("Sword +1", 1));
        Assert.False(new Weapon("Sword +1", 1) == new Weapon("Sword +2", 2));
    }
}
