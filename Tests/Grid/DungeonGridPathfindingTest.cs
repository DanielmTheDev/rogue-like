using GdUnit4;
using Godot;
using RogueLike.Code.Domain.Common;
using RogueLike.Code.Domain.Grid;
using static GdUnit4.Assertions;

namespace RogueLike.Tests.Grid;

/// <summary>
/// Pathfinding is a query over the grid's own cells (walkability + corner-cuts), so it lives on
/// <see cref="DungeonGrid"/> (folded in from the deleted <c>Pathfinder</c> class, 2.3b). The A* work
/// is done by a hidden per-search object; the only public surface is <see cref="DungeonGrid.FindPath"/>.
/// <see cref="DungeonGrid.CanStep"/> is the shared traversability rule (also reused by movement).
/// </summary>
[TestSuite]
public class DungeonGridPathfindingTest
{
    private static DungeonGrid OpenGrid(int w = 10, int h = 10) => new(w, h);

    [TestCase]
    public void CanStep_IntoOpenFloor_ReturnsTrue()
    {
        var grid = OpenGrid();

        AssertBool(grid.CanStep(new GridPos(5, 5), Direction.Right)).IsTrue();
    }

    [TestCase]
    public void CanStep_IntoWall_ReturnsFalse()
    {
        var grid = OpenGrid();
        grid.SetCell(new GridPos(6, 5), CellType.Wall);

        AssertBool(grid.CanStep(new GridPos(5, 5), Direction.Right)).IsFalse();
    }

    [TestCase]
    public void CanStep_DiagonalThatCutsWallCorner_ReturnsFalse()
    {
        var grid = OpenGrid();
        // Wall on one orthogonal side of the (1,1)->(2,2) diagonal.
        grid.SetCell(new GridPos(2, 1), CellType.Wall);

        AssertBool(grid.CanStep(new GridPos(1, 1), Direction.DownRight)).IsFalse();
    }

    [TestCase]
    public void CanStep_OutOfBounds_ReturnsFalse()
    {
        var grid = OpenGrid();

        AssertBool(grid.CanStep(new GridPos(0, 0), Direction.UpLeft)).IsFalse();
    }

    [TestCase]
    public void FindPath_SimpleStraightLine()
    {
        var grid = OpenGrid();

        var path = grid.FindPath(new GridPos(1, 1), new GridPos(1, 5));

        AssertThat(path).IsNotNull();
        AssertThat(path.Count).IsEqual(4);
        AssertThat(path[0]).IsEqual(new GridPos(1, 2));
        AssertThat(path[3]).IsEqual(new GridPos(1, 5));
    }

    [TestCase]
    public void FindPath_AroundWall()
    {
        var grid = OpenGrid();
        grid.SetCell(new GridPos(1, 3), CellType.Wall);

        var path = grid.FindPath(new GridPos(1, 1), new GridPos(1, 5));

        AssertThat(path).IsNotNull();
        // Must not pass through the wall.
        AssertThat(path.Contains(new GridPos(1, 3))).IsFalse();
        // With diagonals the detour is shorter than the old 4-directional path (was >= 6).
        AssertThat(path.Count).IsLessEqual(5);
    }

    [TestCase]
    public void FindPath_OpenDiagonal_TakesDiagonalShortcut()
    {
        var grid = OpenGrid();

        var path = grid.FindPath(new GridPos(1, 1), new GridPos(4, 4));

        AssertThat(path).IsNotNull();
        // Pure diagonal: 3 steps instead of the 6 a cardinal grid would need.
        AssertThat(path.Count).IsEqual(3);
        AssertThat(path[2]).IsEqual(new GridPos(4, 4));
    }

    [TestCase]
    public void FindPath_DiagonalCornerCut_RoutesAround()
    {
        var grid = OpenGrid();
        // Wall on one orthogonal side of the (1,1)->(2,2) diagonal.
        grid.SetCell(new GridPos(2, 1), CellType.Wall);

        var path = grid.FindPath(new GridPos(1, 1), new GridPos(2, 2));

        AssertThat(path).IsNotNull();
        // Cannot cut the corner; must step via (1,2) first.
        AssertThat(path.Count).IsEqual(2);
        AssertThat(path[0]).IsEqual(new GridPos(1, 2));
        AssertThat(path[1]).IsEqual(new GridPos(2, 2));
    }

    [TestCase]
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

        AssertThat(path).IsNull();
    }

    [TestCase]
    public void FindPath_StartAndEndAreSame()
    {
        var grid = OpenGrid();

        var path = grid.FindPath(new GridPos(1, 1), new GridPos(1, 1));

        AssertThat(path).IsNotNull();
        AssertThat(path.Count).IsEqual(0);
    }
}
