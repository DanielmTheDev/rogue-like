using GdUnit4;
using Godot;
using RogueLike.Code.Grid;
using RogueLike.Code.Pathfinding;

namespace RogueLike.Code.Tests.Pathfinding;

[TestSuite]
public class PathfinderTest
{
    private Pathfinder _pathfinder;
    private DungeonGrid _grid;

    [BeforeTest]
    public void Setup()
    {
        _pathfinder = new Pathfinder();
        // Create a 10x10 grid for testing
        _grid = new DungeonGrid(10, 10);
    }

    [TestCase]
    public void FindPath_SimpleStraightLine()
    {
        var start = new Vector2I(1, 1);
        var end = new Vector2I(1, 5);
        var path = _pathfinder.FindPath(start, end, _grid);

        Assertions.AssertThat(path).IsNotNull();
        Assertions.AssertThat(path.Count).IsEqual(4);
        Assertions.AssertThat(path[0]).IsEqual(new Vector2I(1, 2));
        Assertions.AssertThat(path[3]).IsEqual(new Vector2I(1, 5));
    }

    [TestCase]
    public void FindPath_AroundWall()
    {
        // Create a wall at (1, 3)
        _grid.SetCell(new Vector2I(1, 3), CellType.Wall);

        var start = new Vector2I(1, 1);
        var end = new Vector2I(1, 5);
        var path = _pathfinder.FindPath(start, end, _grid);

        Assertions.AssertThat(path).IsNotNull();
        // Must not pass through the wall.
        Assertions.AssertThat(path.Contains(new Vector2I(1, 3))).IsFalse();
        // With diagonals the detour is shorter than the old 4-directional path (was >= 6).
        Assertions.AssertThat(path.Count).IsLessEqual(5);
    }

    [TestCase]
    public void FindPath_OpenDiagonal_TakesDiagonalShortcut()
    {
        var start = new Vector2I(1, 1);
        var end = new Vector2I(4, 4);
        var path = _pathfinder.FindPath(start, end, _grid);

        Assertions.AssertThat(path).IsNotNull();
        // Pure diagonal: 3 steps instead of the 6 a cardinal grid would need.
        Assertions.AssertThat(path.Count).IsEqual(3);
        Assertions.AssertThat(path[2]).IsEqual(new Vector2I(4, 4));
    }

    [TestCase]
    public void FindPath_DiagonalCornerCut_RoutesAround()
    {
        // Wall on one orthogonal side of the (1,1)->(2,2) diagonal.
        _grid.SetCell(new Vector2I(2, 1), CellType.Wall);

        var start = new Vector2I(1, 1);
        var end = new Vector2I(2, 2);
        var path = _pathfinder.FindPath(start, end, _grid);

        Assertions.AssertThat(path).IsNotNull();
        // Cannot cut the corner; must step via (1,2) first.
        Assertions.AssertThat(path.Count).IsEqual(2);
        Assertions.AssertThat(path[0]).IsEqual(new Vector2I(1, 2));
        Assertions.AssertThat(path[1]).IsEqual(new Vector2I(2, 2));
    }

    [TestCase]
    public void FindPath_NoPathExists()
    {
        // Wall off the target completely
        _grid.SetCell(new Vector2I(0, 4), CellType.Wall);
        _grid.SetCell(new Vector2I(1, 4), CellType.Wall);
        _grid.SetCell(new Vector2I(2, 4), CellType.Wall);
        _grid.SetCell(new Vector2I(0, 5), CellType.Wall);
        _grid.SetCell(new Vector2I(2, 5), CellType.Wall);
        _grid.SetCell(new Vector2I(0, 6), CellType.Wall);
        _grid.SetCell(new Vector2I(1, 6), CellType.Wall);
        _grid.SetCell(new Vector2I(2, 6), CellType.Wall);

        var start = new Vector2I(1, 1);
        var end = new Vector2I(1, 5);
        var path = _pathfinder.FindPath(start, end, _grid);

        Assertions.AssertThat(path).IsNull();
    }

    [TestCase]
    public void FindPath_StartAndEndAreSame()
    {
        var start = new Vector2I(1, 1);
        var end = new Vector2I(1, 1);
        var path = _pathfinder.FindPath(start, end, _grid);

        Assertions.AssertThat(path).IsNotNull();
        Assertions.AssertThat(path.Count).IsEqual(0);
    }
}
