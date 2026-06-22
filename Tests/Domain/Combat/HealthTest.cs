using GdUnit4;
using System;
using RogueLike.Code.Domain.Combat;
using static GdUnit4.Assertions;

namespace RogueLike.Tests.Domain.Combat;

[TestSuite]
public class HealthTest
{
    [TestCase]
    public void NewHealth_IsFull_AndAlive()
    {
        var health = new Health(10, 10);

        AssertInt(health.Current).IsEqual(10);
        AssertInt(health.Max).IsEqual(10);
        AssertBool(health.IsDead).IsFalse();
    }

    [TestCase]
    public void TakeDamage_ReducesCurrent()
    {
        var health = new Health(10, 10).TakeDamage(3);

        AssertInt(health.Current).IsEqual(7);
        AssertInt(health.Max).IsEqual(10);
    }

    [TestCase]
    public void TakeDamage_ClampsAtZero_AndIsDead()
    {
        var health = new Health(5, 10).TakeDamage(99);

        AssertInt(health.Current).IsEqual(0);
        AssertBool(health.IsDead).IsTrue();
    }

    [TestCase]
    public void TakeDamage_NonPositive_IsNoOp()
    {
        var health = new Health(8, 10).TakeDamage(0);

        AssertInt(health.Current).IsEqual(8);
    }

    [TestCase]
    public void Heal_ClampsAtMax()
    {
        var health = new Health(8, 10).Heal(99);

        AssertInt(health.Current).IsEqual(10);
    }

    [TestCase]
    public void Heal_OnDead_IsNoOp()
    {
        var health = new Health(0, 10).Heal(5);

        AssertInt(health.Current).IsEqual(0);
        AssertBool(health.IsDead).IsTrue();
    }

    [TestCase]
    public void WithIncreasedMax_RaisesMax_AndHealsToFull()
    {
        var health = new Health(3, 10).WithIncreasedMax(5);

        AssertInt(health.Max).IsEqual(15);
        AssertInt(health.Current).IsEqual(15);
    }

    [TestCase]
    public void Ctor_CurrentAboveMax_Throws()
    {
        AssertThrown(() => new Health(11, 10)).IsInstanceOf<ArgumentOutOfRangeException>();
    }

    [TestCase]
    public void Ctor_NonPositiveMax_Throws()
    {
        AssertThrown(() => new Health(0, 0)).IsInstanceOf<ArgumentOutOfRangeException>();
    }

    [TestCase]
    public void EqualityByValue()
    {
        AssertBool(new Health(7, 10) == new Health(7, 10)).IsTrue();
        AssertBool(new Health(7, 10) == new Health(8, 10)).IsFalse();
    }
}
