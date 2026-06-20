using System.Linq;
using GdUnit4;
using RogueLike.Code.Domain.Common;
using static GdUnit4.Assertions;

namespace RogueLike.Tests.Domain.Common;

[TestSuite]
public class DirectionTest
{
    [TestCase]
    public void FromDelta_StoresComponents()
    {
        var dir = Direction.FromDelta(1, -1);

        AssertInt(dir.Dx).IsEqual(1);
        AssertInt(dir.Dy).IsEqual(-1);
    }

    [TestCase]
    public void FromDelta_OutOfRange_Throws()
    {
        var threw = false;
        try { _ = Direction.FromDelta(2, 0); }
        catch (System.ArgumentOutOfRangeException) { threw = true; }

        AssertBool(threw).IsTrue();
    }

    [TestCase]
    public void NamedStatics_HaveExpectedDeltas()
    {
        AssertBool(Direction.Up == Direction.FromDelta(0, -1)).IsTrue();
        AssertBool(Direction.Right == Direction.FromDelta(1, 0)).IsTrue();
        AssertBool(Direction.DownLeft == Direction.FromDelta(-1, 1)).IsTrue();
    }

    [TestCase]
    public void Default_IsNone()
    {
        AssertBool(default(Direction) == Direction.None).IsTrue();
    }

    [TestCase]
    public void None_IsZero()
    {
        AssertBool(Direction.None.IsZero).IsTrue();
        AssertBool(Direction.Right.IsZero).IsFalse();
    }

    [TestCase]
    public void IsDiagonal_OnlyWhenBothAxesNonZero()
    {
        AssertBool(Direction.DownRight.IsDiagonal).IsTrue();
        AssertBool(Direction.Right.IsDiagonal).IsFalse();
        AssertBool(Direction.None.IsDiagonal).IsFalse();
    }

    [TestCase]
    public void AllEight_HasEightDistinctNonZeroDirections()
    {
        AssertInt(Direction.AllEight.Count).IsEqual(8);
        AssertBool(Direction.AllEight.Any(d => d.IsZero)).IsFalse();
        AssertInt(Direction.AllEight.Distinct().Count()).IsEqual(8);
    }

    [TestCase]
    public void Equality_IsByValue()
    {
        AssertBool(Direction.FromDelta(1, -1) == Direction.UpRight).IsTrue();
        AssertBool(Direction.FromDelta(1, -1) == Direction.DownLeft).IsFalse();
    }
}
