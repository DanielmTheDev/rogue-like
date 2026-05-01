using System.Collections.Generic;
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
        // Expected path: (1,2) -> (0,2) or (2,2) -> (0,3) or (2,3) etc.
        // The exact path can vary, but it must not contain the wall.
        Assertions.AssertThat(path.Contains(new Vector2I(1, 3))).IsFalse();
        Assertions.AssertThat(path.Count).IsGreaterEqual(6); // Path is longer now
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
