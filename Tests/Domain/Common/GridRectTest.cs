using GdUnit4;
using System;
using RogueLike.Code.Domain.Common;
using static GdUnit4.Assertions;

namespace RogueLike.Tests.Domain.Common;

[TestSuite]
public class GridRectTest
{
    [TestCase]
    public void Construct_FromInts_SetsOriginAndSize()
    {
        var rect = new GridRect(2, 3, 5, 4);

        AssertInt(rect.X).IsEqual(2);
        AssertInt(rect.Y).IsEqual(3);
        AssertInt(rect.Width).IsEqual(5);
        AssertInt(rect.Height).IsEqual(4);
        AssertBool(rect.Position == new GridPos(2, 3)).IsTrue();
    }

    [TestCase]
    public void End_IsExclusiveBottomRight()
    {
        var rect = new GridRect(2, 3, 5, 4);

        AssertBool(rect.End == new GridPos(7, 7)).IsTrue();
    }

    [TestCase]
    public void Center_IsIntegerMidpoint()
    {
        var rect = new GridRect(0, 0, 4, 6);

        AssertBool(rect.Center == new GridPos(2, 3)).IsTrue();
    }

    [TestCase]
    public void Ctor_NonPositiveSize_Throws()
    {
        AssertThrown(() => new GridRect(0, 0, 0, 5)).IsInstanceOf<ArgumentOutOfRangeException>();
        AssertThrown(() => new GridRect(0, 0, 5, -1)).IsInstanceOf<ArgumentOutOfRangeException>();
    }

    [TestCase]
    public void EqualityByValue()
    {
        AssertBool(new GridRect(1, 2, 3, 4) == new GridRect(1, 2, 3, 4)).IsTrue();
        AssertBool(new GridRect(1, 2, 3, 4) == new GridRect(1, 2, 3, 5)).IsFalse();
    }
}
