using System;
using Xunit;
using RogueLike.Domain.Common;

namespace RogueLike.Domain.Tests.Common;

public class GridRectTest
{
    [Fact]
    public void Construct_FromInts_SetsOriginAndSize()
    {
        var rect = new GridRect(2, 3, 5, 4);

        Assert.Equal(2, rect.X);
        Assert.Equal(3, rect.Y);
        Assert.Equal(5, rect.Width);
        Assert.Equal(4, rect.Height);
        Assert.True(rect.Position == new GridPos(2, 3));
    }

    [Fact]
    public void End_IsExclusiveBottomRight()
    {
        var rect = new GridRect(2, 3, 5, 4);

        Assert.True(rect.End == new GridPos(7, 7));
    }

    [Fact]
    public void Center_IsIntegerMidpoint()
    {
        var rect = new GridRect(0, 0, 4, 6);

        Assert.True(rect.Center == new GridPos(2, 3));
    }

    [Fact]
    public void Ctor_NonPositiveSize_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new GridRect(0, 0, 0, 5));
        Assert.Throws<ArgumentOutOfRangeException>(() => new GridRect(0, 0, 5, -1));
    }

    [Fact]
    public void EqualityByValue()
    {
        Assert.True(new GridRect(1, 2, 3, 4) == new GridRect(1, 2, 3, 4));
        Assert.False(new GridRect(1, 2, 3, 4) == new GridRect(1, 2, 3, 5));
    }
}
