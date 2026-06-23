using System;
using Xunit;
using RogueLike.Domain.Combat;

namespace RogueLike.Domain.Tests.Combat;

public class HealthTest
{
    [Fact]
    public void NewHealth_IsFull_AndAlive()
    {
        var health = new Health(10, 10);

        Assert.Equal(10, health.Current);
        Assert.Equal(10, health.Max);
        Assert.False(health.IsDead);
    }

    [Fact]
    public void TakeDamage_ReducesCurrent()
    {
        var health = new Health(10, 10).TakeDamage(3);

        Assert.Equal(7, health.Current);
        Assert.Equal(10, health.Max);
    }

    [Fact]
    public void TakeDamage_ClampsAtZero_AndIsDead()
    {
        var health = new Health(5, 10).TakeDamage(99);

        Assert.Equal(0, health.Current);
        Assert.True(health.IsDead);
    }

    [Fact]
    public void TakeDamage_NonPositive_IsNoOp()
    {
        var health = new Health(8, 10).TakeDamage(0);

        Assert.Equal(8, health.Current);
    }

    [Fact]
    public void Heal_ClampsAtMax()
    {
        var health = new Health(8, 10).Heal(99);

        Assert.Equal(10, health.Current);
    }

    [Fact]
    public void Heal_OnDead_IsNoOp()
    {
        var health = new Health(0, 10).Heal(5);

        Assert.Equal(0, health.Current);
        Assert.True(health.IsDead);
    }

    [Fact]
    public void WithIncreasedMax_RaisesMax_AndHealsToFull()
    {
        var health = new Health(3, 10).WithIncreasedMax(5);

        Assert.Equal(15, health.Max);
        Assert.Equal(15, health.Current);
    }

    [Fact]
    public void Ctor_CurrentAboveMax_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Health(11, 10));
    }

    [Fact]
    public void Ctor_NonPositiveMax_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Health(0, 0));
    }

    [Fact]
    public void EqualityByValue()
    {
        Assert.True(new Health(7, 10) == new Health(7, 10));
        Assert.False(new Health(7, 10) == new Health(8, 10));
    }
}
