using System.Collections.Generic;
using GdUnit4;
using RogueLike.Code.Domain.Common;
using static GdUnit4.Assertions;

namespace RogueLike.Tests.Domain.Common;

[TestSuite]
public class GridPosTest
{
    [TestCase]
    public void Constructor_StoresCoordinates()
    {
        var pos = new GridPos(3, 5);

        AssertInt(pos.X).IsEqual(3);
        AssertInt(pos.Y).IsEqual(5);
    }

    [TestCase]
    public void Step_AppliesDirection()
    {
        var moved = new GridPos(2, 2).Step(Direction.UpRight);

        AssertBool(moved == new GridPos(3, 1)).IsTrue();
    }

    [TestCase]
    public void ManhattanTo_SumsAxisDistances()
    {
        AssertInt(new GridPos(1, 1).ManhattanTo(new GridPos(4, 3))).IsEqual(5);
    }

    [TestCase]
    public void ChebyshevTo_TakesMaxAxisDistance()
    {
        AssertInt(new GridPos(1, 1).ChebyshevTo(new GridPos(4, 3))).IsEqual(3);
    }

    [TestCase]
    public void DirectionTo_IsUnitStepTowardsTarget()
    {
        var dir = new GridPos(2, 2).DirectionTo(new GridPos(5, 0));

        AssertBool(dir == Direction.UpRight).IsTrue();
    }

    [TestCase]
    public void Equality_AndHashing_WorkAsDictionaryKey()
    {
        var map = new Dictionary<GridPos, string> { [new GridPos(2, 3)] = "here" };

        AssertBool(map.ContainsKey(new GridPos(2, 3))).IsTrue();
        AssertString(map[new GridPos(2, 3)]).IsEqual("here");
        AssertBool(map.ContainsKey(new GridPos(3, 2))).IsFalse();
    }
}
