using System.Collections.Generic;
using Xunit;
using RogueLike.Domain.Common;

namespace RogueLike.Domain.Tests.Common;

public class GridPosTest
{
    [Fact]
    public void Constructor_StoresCoordinates()
    {
        var pos = new GridPos(3, 5);

        Assert.Equal(3, pos.X);
        Assert.Equal(5, pos.Y);
    }

    [Fact]
    public void Step_AppliesDirection()
    {
        var moved = new GridPos(2, 2).Step(Direction.UpRight);

        Assert.True(moved == new GridPos(3, 1));
    }

    [Fact]
    public void ManhattanTo_SumsAxisDistances()
    {
        Assert.Equal(5, new GridPos(1, 1).ManhattanTo(new GridPos(4, 3)));
    }

    [Fact]
    public void ChebyshevTo_TakesMaxAxisDistance()
    {
        Assert.Equal(3, new GridPos(1, 1).ChebyshevTo(new GridPos(4, 3)));
    }

    [Fact]
    public void DirectionTo_IsUnitStepTowardsTarget()
    {
        var dir = new GridPos(2, 2).DirectionTo(new GridPos(5, 0));

        Assert.True(dir == Direction.UpRight);
    }

    [Fact]
    public void Equality_AndHashing_WorkAsDictionaryKey()
    {
        var map = new Dictionary<GridPos, string> { [new GridPos(2, 3)] = "here" };

        Assert.True(map.ContainsKey(new GridPos(2, 3)));
        Assert.Equal("here", map[new GridPos(2, 3)]);
        Assert.False(map.ContainsKey(new GridPos(3, 2)));
    }
}
