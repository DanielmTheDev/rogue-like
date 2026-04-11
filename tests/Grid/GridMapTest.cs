using GdUnit4;
using Godot;
using RogueLike.Code.Grid;
using static GdUnit4.Assertions;

namespace RogueLike.tests.Grid;

[TestSuite]
public class DungeonGridTest
{
    [TestCase]
    public void IsInBounds_InsideGrid_ReturnsTrue()
    {
        var grid = new DungeonGrid(10, 8);

        AssertBool(grid.IsInBounds(new Vector2I(0, 0))).IsTrue();
        AssertBool(grid.IsInBounds(new Vector2I(5, 4))).IsTrue();
        AssertBool(grid.IsInBounds(new Vector2I(9, 7))).IsTrue();
    }

    [TestCase]
    public void IsInBounds_OutsideGrid_ReturnsFalse()
    {
        var grid = new DungeonGrid(10, 8);

        AssertBool(grid.IsInBounds(new Vector2I(-1, 0))).IsFalse();
        AssertBool(grid.IsInBounds(new Vector2I(0, -1))).IsFalse();
        AssertBool(grid.IsInBounds(new Vector2I(10, 0))).IsFalse();
        AssertBool(grid.IsInBounds(new Vector2I(0, 8))).IsFalse();
    }

    [TestCase]
    public void IsWalkable_FloorCell_ReturnsTrue()
    {
        var grid = new DungeonGrid(10, 8);

        // Default fill is Floor
        AssertBool(grid.IsWalkable(new Vector2I(3, 3))).IsTrue();
    }

    [TestCase]
    public void IsWalkable_WallCell_ReturnsFalse()
    {
        var grid = new DungeonGrid(10, 8);
        grid.SetCell(new Vector2I(3, 3), CellType.Wall);

        AssertBool(grid.IsWalkable(new Vector2I(3, 3))).IsFalse();
    }

    [TestCase]
    public void IsWalkable_OutOfBounds_ReturnsFalse()
    {
        var grid = new DungeonGrid(10, 8);

        AssertBool(grid.IsWalkable(new Vector2I(-1, 0))).IsFalse();
        AssertBool(grid.IsWalkable(new Vector2I(10, 0))).IsFalse();
    }

    [TestCase]
    public void GridToWorld_ReturnsCorrectCenter()
    {
        var grid = new DungeonGrid(10, 8, 32);

        // Tile (0,0) center should be at (16, 16)
        var worldPos = grid.GridToWorld(new Vector2I(0, 0));
        AssertFloat(worldPos.X).IsEqual(16f);
        AssertFloat(worldPos.Y).IsEqual(16f);

        // Tile (1,0) center should be at (48, 16)
        var worldPos2 = grid.GridToWorld(new Vector2I(1, 0));
        AssertFloat(worldPos2.X).IsEqual(48f);
        AssertFloat(worldPos2.Y).IsEqual(16f);
    }

    [TestCase]
    public void WorldToGrid_ReturnsCorrectCoord()
    {
        var grid = new DungeonGrid(10, 8, 32);

        // World position (16, 16) -> grid (0, 0)
        var coord = grid.WorldToGrid(new Vector2(16f, 16f));
        AssertInt(coord.X).IsEqual(0);
        AssertInt(coord.Y).IsEqual(0);

        // World position (48, 16) -> grid (1, 0)
        var coord2 = grid.WorldToGrid(new Vector2(48f, 16f));
        AssertInt(coord2.X).IsEqual(1);
        AssertInt(coord2.Y).IsEqual(0);
    }

    [TestCase]
    public void GetCell_OutOfBounds_ReturnsWall()
    {
        var grid = new DungeonGrid(10, 8);

        AssertObject(grid.GetCell(new Vector2I(-1, 0)))
            .IsEqual(CellType.Wall);
    }

    [TestCase]
    public void SetCell_ThenGetCell_ReturnsCorrectType()
    {
        var grid = new DungeonGrid(10, 8);
        grid.SetCell(new Vector2I(2, 3), CellType.Wall);

        AssertObject(grid.GetCell(new Vector2I(2, 3)))
            .IsEqual(CellType.Wall);
    }
}
