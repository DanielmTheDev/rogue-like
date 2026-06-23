using System;
using System.Linq;
using Xunit;
using RogueLike.Domain.Common;

namespace RogueLike.Domain.Tests.Common;

public class DirectionTest
{
    [Fact]
    public void FromDelta_StoresComponents()
    {
        var dir = Direction.FromDelta(1, -1);

        Assert.Equal(1, dir.Dx);
        Assert.Equal(-1, dir.Dy);
    }

    [Fact]
    public void FromDelta_OutOfRange_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Direction.FromDelta(2, 0));
    }

    [Fact]
    public void NamedStatics_HaveExpectedDeltas()
    {
        Assert.True(Direction.Up == Direction.FromDelta(0, -1));
        Assert.True(Direction.Right == Direction.FromDelta(1, 0));
        Assert.True(Direction.DownLeft == Direction.FromDelta(-1, 1));
    }

    [Fact]
    public void Default_IsNone()
    {
        Assert.True(default(Direction) == Direction.None);
    }

    [Fact]
    public void None_IsZero()
    {
        Assert.True(Direction.None.IsZero);
        Assert.False(Direction.Right.IsZero);
    }

    [Fact]
    public void IsDiagonal_OnlyWhenBothAxesNonZero()
    {
        Assert.True(Direction.DownRight.IsDiagonal);
        Assert.False(Direction.Right.IsDiagonal);
        Assert.False(Direction.None.IsDiagonal);
    }

    [Fact]
    public void AllEight_HasEightDistinctNonZeroDirections()
    {
        Assert.Equal(8, Direction.AllEight.Count);
        Assert.DoesNotContain(Direction.AllEight, d => d.IsZero);
        Assert.Equal(8, Direction.AllEight.Distinct().Count());
    }

    [Fact]
    public void Equality_IsByValue()
    {
        Assert.True(Direction.FromDelta(1, -1) == Direction.UpRight);
        Assert.False(Direction.FromDelta(1, -1) == Direction.DownLeft);
    }
}
