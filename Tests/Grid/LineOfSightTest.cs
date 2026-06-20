using GdUnit4;
using Godot;
using RogueLike.Code.Domain.Common;
using RogueLike.Code.Grid;
using static GdUnit4.Assertions;

namespace RogueLike.Tests.Grid;

/// <summary>
/// Line-of-sight is a query about the grid (the grid owns the walls), so it lives on
/// <see cref="DungeonGrid.HasClearLine"/>. Endpoints are excluded from the wall check.
/// </summary>
[TestSuite]
public class LineOfSightTest
{
    private static DungeonGrid OpenGrid(int w = 10, int h = 10) => new(w, h);

    [TestCase]
    public void HasClearLine_OnOpenHorizontalRow_ReturnsTrue()
    {
        var grid = OpenGrid();

        AssertBool(grid.HasClearLine(new GridPos(1, 5), new GridPos(8, 5))).IsTrue();
    }

    [TestCase]
    public void HasClearLine_WhenWallBlocksRow_ReturnsFalse()
    {
        var grid = OpenGrid();
        grid.SetCell(new Vector2I(4, 5), CellType.Wall);

        AssertBool(grid.HasClearLine(new GridPos(1, 5), new GridPos(8, 5))).IsFalse();
    }

    [TestCase]
    public void HasClearLine_AdjacentTiles_ReturnsTrue()
    {
        var grid = OpenGrid();

        AssertBool(grid.HasClearLine(new GridPos(2, 2), new GridPos(3, 2))).IsTrue();
    }

    [TestCase]
    public void HasClearLine_OriginEqualsTarget_ReturnsTrue()
    {
        var grid = OpenGrid();

        AssertBool(grid.HasClearLine(new GridPos(2, 2), new GridPos(2, 2))).IsTrue();
    }

    [TestCase]
    public void HasClearLine_WallOnTargetTile_StillReturnsTrue()
    {
        // Endpoints are excluded from the wall check.
        var grid = OpenGrid();
        grid.SetCell(new Vector2I(8, 5), CellType.Wall);

        AssertBool(grid.HasClearLine(new GridPos(1, 5), new GridPos(8, 5))).IsTrue();
    }

    [TestCase]
    public void HasClearLine_ClearDiagonal_ReturnsTrue()
    {
        var grid = OpenGrid();

        AssertBool(grid.HasClearLine(new GridPos(1, 1), new GridPos(5, 5))).IsTrue();
    }

    [TestCase]
    public void HasClearLine_WallOnDiagonalPath_ReturnsFalse()
    {
        var grid = OpenGrid();
        grid.SetCell(new Vector2I(3, 3), CellType.Wall);

        AssertBool(grid.HasClearLine(new GridPos(1, 1), new GridPos(5, 5))).IsFalse();
    }
}
