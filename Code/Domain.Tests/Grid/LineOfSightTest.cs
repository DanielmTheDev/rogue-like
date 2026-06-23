using Xunit;
using RogueLike.Domain.Common;
using RogueLike.Domain.Grid;

namespace RogueLike.Domain.Tests.Grid;

/// <summary>
/// Line-of-sight is a query about the grid (the grid owns the walls), so it lives on
/// <see cref="DungeonGrid.HasClearLine"/>. Endpoints are excluded from the wall check.
/// </summary>
public class LineOfSightTest
{
    private static DungeonGrid OpenGrid(int w = 10, int h = 10) => new(w, h);

    [Fact]
    public void HasClearLine_OnOpenHorizontalRow_ReturnsTrue()
    {
        var grid = OpenGrid();

        Assert.True(grid.HasClearLine(new GridPos(1, 5), new GridPos(8, 5)));
    }

    [Fact]
    public void HasClearLine_WhenWallBlocksRow_ReturnsFalse()
    {
        var grid = OpenGrid();
        grid.SetCell(new GridPos(4, 5), CellType.Wall);

        Assert.False(grid.HasClearLine(new GridPos(1, 5), new GridPos(8, 5)));
    }

    [Fact]
    public void HasClearLine_AdjacentTiles_ReturnsTrue()
    {
        var grid = OpenGrid();

        Assert.True(grid.HasClearLine(new GridPos(2, 2), new GridPos(3, 2)));
    }

    [Fact]
    public void HasClearLine_OriginEqualsTarget_ReturnsTrue()
    {
        var grid = OpenGrid();

        Assert.True(grid.HasClearLine(new GridPos(2, 2), new GridPos(2, 2)));
    }

    [Fact]
    public void HasClearLine_WallOnTargetTile_StillReturnsTrue()
    {
        // Endpoints are excluded from the wall check.
        var grid = OpenGrid();
        grid.SetCell(new GridPos(8, 5), CellType.Wall);

        Assert.True(grid.HasClearLine(new GridPos(1, 5), new GridPos(8, 5)));
    }

    [Fact]
    public void HasClearLine_ClearDiagonal_ReturnsTrue()
    {
        var grid = OpenGrid();

        Assert.True(grid.HasClearLine(new GridPos(1, 1), new GridPos(5, 5)));
    }

    [Fact]
    public void HasClearLine_WallOnDiagonalPath_ReturnsFalse()
    {
        var grid = OpenGrid();
        grid.SetCell(new GridPos(3, 3), CellType.Wall);

        Assert.False(grid.HasClearLine(new GridPos(1, 1), new GridPos(5, 5)));
    }
}
