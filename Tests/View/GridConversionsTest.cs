using GdUnit4;
using Godot;
using RogueLike.Code.Domain.Common;
using RogueLike.Code.View;
using static GdUnit4.Assertions;

namespace RogueLike.Tests.View;

[TestSuite]
public class GridConversionsTest
{
    [TestCase]
    public void ToGridPos_FromVector2I_CopiesComponents()
    {
        var p = new Vector2I(3, 7).ToGridPos();

        AssertInt(p.X).IsEqual(3);
        AssertInt(p.Y).IsEqual(7);
    }

    [TestCase]
    public void ToVector2I_FromGridPos_CopiesComponents()
    {
        var v = new GridPos(3, 7).ToVector2I();

        AssertInt(v.X).IsEqual(3);
        AssertInt(v.Y).IsEqual(7);
    }

    [TestCase]
    public void TypeRoundTrip_IsIdentity()
    {
        var original = new GridPos(4, 9);

        AssertObject(original.ToVector2I().ToGridPos()).IsEqual(original);
    }

    [TestCase]
    public void ToWorldCenter_ReturnsTileCenter()
    {
        // Tile (0,0) center -> (16,16) at tileSize 32.
        var c0 = new GridPos(0, 0).ToWorldCenter(32);
        AssertFloat(c0.X).IsEqual(16f);
        AssertFloat(c0.Y).IsEqual(16f);

        // Tile (1,0) center -> (48,16).
        var c1 = new GridPos(1, 0).ToWorldCenter(32);
        AssertFloat(c1.X).IsEqual(48f);
        AssertFloat(c1.Y).IsEqual(16f);
    }

    [TestCase]
    public void ToGridPos_FromWorld_FloorsToTile()
    {
        // World (16,16) -> grid (0,0).
        var g0 = new Vector2(16f, 16f).ToGridPos(32);
        AssertInt(g0.X).IsEqual(0);
        AssertInt(g0.Y).IsEqual(0);

        // World (48,16) -> grid (1,0).
        var g1 = new Vector2(48f, 16f).ToGridPos(32);
        AssertInt(g1.X).IsEqual(1);
        AssertInt(g1.Y).IsEqual(0);
    }
}
