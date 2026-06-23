using Xunit;
using RogueLike.Domain.Common;
using RogueLike.Domain.Grid;

namespace RogueLike.Domain.Tests.Grid;

/// <summary>
/// Pathfinding is a query over the grid's own cells (walkability + corner-cuts), so it lives on
/// <see cref="DungeonGrid"/> (folded in from the deleted <c>Pathfinder</c> class, 2.3b). The A* work
/// is done by a hidden per-search object; the only public surface is <see cref="DungeonGrid.FindPath"/>.
/// <see cref="DungeonGrid.CanStep"/> is the shared traversability rule (also reused by movement).
/// </summary>
public class DungeonGridPathfindingTest
{
    private static DungeonGrid OpenGrid(int w = 10, int h = 10) => new(w, h);

    [Fact]
    public void CanStep_IntoOpenFloor_ReturnsTrue()
    {
        var grid = OpenGrid();

        Assert.True(grid.CanStep(new GridPos(5, 5), Direction.Right));
    }

    [Fact]
    public void CanStep_IntoWall_ReturnsFalse()
    {
        var grid = OpenGrid();
        grid.SetCell(new GridPos(6, 5), CellType.Wall);

        Assert.False(grid.CanStep(new GridPos(5, 5), Direction.Right));
    }

    [Fact]
    public void CanStep_DiagonalThatCutsWallCorner_ReturnsFalse()
    {
        var grid = OpenGrid();
        // Wall on one orthogonal side of the (1,1)->(2,2) diagonal.
        grid.SetCell(new GridPos(2, 1), CellType.Wall);

        Assert.False(grid.CanStep(new GridPos(1, 1), Direction.DownRight));
    }

    [Fact]
    public void CanStep_OutOfBounds_ReturnsFalse()
    {
        var grid = OpenGrid();

        Assert.False(grid.CanStep(new GridPos(0, 0), Direction.UpLeft));
    }

    [Fact]
    public void FindPath_SimpleStraightLine()
    {
        var grid = OpenGrid();

        var path = grid.FindPath(new GridPos(1, 1), new GridPos(1, 5));

        Assert.NotNull(path);
        Assert.Equal(4, path.Count);
        Assert.Equal(new GridPos(1, 2), path[0]);
        Assert.Equal(new GridPos(1, 5), path[3]);
    }

    [Fact]
    public void FindPath_AroundWall()
    {
        var grid = OpenGrid();
        grid.SetCell(new GridPos(1, 3), CellType.Wall);

        var path = grid.FindPath(new GridPos(1, 1), new GridPos(1, 5));

        Assert.NotNull(path);
        // Must not pass through the wall.
        Assert.DoesNotContain(new GridPos(1, 3), path);
        // With diagonals the detour is shorter than the old 4-directional path (was >= 6).
        Assert.True(path.Count <= 5);
    }

    [Fact]
    public void FindPath_OpenDiagonal_TakesDiagonalShortcut()
    {
        var grid = OpenGrid();

        var path = grid.FindPath(new GridPos(1, 1), new GridPos(4, 4));

        Assert.NotNull(path);
        // Pure diagonal: 3 steps instead of the 6 a cardinal grid would need.
        Assert.Equal(3, path.Count);
        Assert.Equal(new GridPos(4, 4), path[2]);
    }

    [Fact]
    public void FindPath_DiagonalCornerCut_RoutesAround()
    {
        var grid = OpenGrid();
        // Wall on one orthogonal side of the (1,1)->(2,2) diagonal.
        grid.SetCell(new GridPos(2, 1), CellType.Wall);

        var path = grid.FindPath(new GridPos(1, 1), new GridPos(2, 2));

        Assert.NotNull(path);
        // Cannot cut the corner; must step via (1,2) first.
        Assert.Equal(2, path.Count);
        Assert.Equal(new GridPos(1, 2), path[0]);
        Assert.Equal(new GridPos(2, 2), path[1]);
    }

    [Fact]
    public void FindPath_NoPathExists()
    {
        var grid = OpenGrid();
        // Wall off the target completely.
        grid.SetCell(new GridPos(0, 4), CellType.Wall);
        grid.SetCell(new GridPos(1, 4), CellType.Wall);
        grid.SetCell(new GridPos(2, 4), CellType.Wall);
        grid.SetCell(new GridPos(0, 5), CellType.Wall);
        grid.SetCell(new GridPos(2, 5), CellType.Wall);
        grid.SetCell(new GridPos(0, 6), CellType.Wall);
        grid.SetCell(new GridPos(1, 6), CellType.Wall);
        grid.SetCell(new GridPos(2, 6), CellType.Wall);

        var path = grid.FindPath(new GridPos(1, 1), new GridPos(1, 5));

        Assert.Null(path);
    }

    [Fact]
    public void FindPath_StartAndEndAreSame()
    {
        var grid = OpenGrid();

        var path = grid.FindPath(new GridPos(1, 1), new GridPos(1, 1));

        Assert.NotNull(path);
        Assert.Empty(path);
    }
}
