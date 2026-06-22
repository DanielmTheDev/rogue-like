using GdUnit4;
using RogueLike.Code.Domain.Common;
using RogueLike.Code.Domain.Grid;
using static GdUnit4.Assertions;

namespace RogueLike.Tests.Grid;

[TestSuite]
public class DungeonGridTest
{
    [TestCase]
    public void IsInBounds_InsideGrid_ReturnsTrue()
    {
        var grid = new DungeonGrid(10, 8);

        AssertBool(grid.IsInBounds(new GridPos(0, 0))).IsTrue();
        AssertBool(grid.IsInBounds(new GridPos(5, 4))).IsTrue();
        AssertBool(grid.IsInBounds(new GridPos(9, 7))).IsTrue();
    }

    [TestCase]
    public void IsInBounds_OutsideGrid_ReturnsFalse()
    {
        var grid = new DungeonGrid(10, 8);

        AssertBool(grid.IsInBounds(new GridPos(-1, 0))).IsFalse();
        AssertBool(grid.IsInBounds(new GridPos(0, -1))).IsFalse();
        AssertBool(grid.IsInBounds(new GridPos(10, 0))).IsFalse();
        AssertBool(grid.IsInBounds(new GridPos(0, 8))).IsFalse();
    }

    [TestCase]
    public void IsWalkable_FloorCell_ReturnsTrue()
    {
        var grid = new DungeonGrid(10, 8);

        // Default fill is Floor
        AssertBool(grid.IsWalkable(new GridPos(3, 3))).IsTrue();
    }

    [TestCase]
    public void IsWalkable_WallCell_ReturnsFalse()
    {
        var grid = new DungeonGrid(10, 8);
        grid.SetCell(new GridPos(3, 3), CellType.Wall);

        AssertBool(grid.IsWalkable(new GridPos(3, 3))).IsFalse();
    }

    [TestCase]
    public void IsWalkable_OutOfBounds_ReturnsFalse()
    {
        var grid = new DungeonGrid(10, 8);

        AssertBool(grid.IsWalkable(new GridPos(-1, 0))).IsFalse();
        AssertBool(grid.IsWalkable(new GridPos(10, 0))).IsFalse();
    }

    [TestCase]
    public void GetCell_OutOfBounds_ReturnsWall()
    {
        var grid = new DungeonGrid(10, 8);

        AssertObject(grid.GetCell(new GridPos(-1, 0)))
            .IsEqual(CellType.Wall);
    }

    [TestCase]
    public void SetCell_ThenGetCell_ReturnsCorrectType()
    {
        var grid = new DungeonGrid(10, 8);
        grid.SetCell(new GridPos(2, 3), CellType.Wall);

        AssertObject(grid.GetCell(new GridPos(2, 3)))
            .IsEqual(CellType.Wall);
    }
}
