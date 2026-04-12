using GdUnit4;
using Godot;
using RogueLike.Code.Grid;
using RogueLike.Code.Grid.FOV;
using static GdUnit4.Assertions;

namespace RogueLike.tests.Grid.FOV;

[TestSuite]
public class RaycasterTest
{
    [TestCase]
    public void ComputeFov_ClearsPreviousVisibleTiles()
    {
        var grid = new DungeonGrid(10, 10, 32);
        var fov = new FovMap(10, 10);
        var raycaster = new Raycaster();

        // compute first time
        raycaster.ComputeFov(fov, grid, new Vector2I(5, 5), 3);
        AssertInt((int)fov.GetVisibility(new Vector2I(5, 6))).IsEqual((int)VisibilityState.Visible);

        // move away
        raycaster.ComputeFov(fov, grid, new Vector2I(1, 1), 1);
        
        // previous tiles should now be Explored, not Visible
        AssertInt((int)fov.GetVisibility(new Vector2I(5, 6))).IsEqual((int)VisibilityState.Explored);
    }

    [TestCase]
    public void ComputeFov_BlockedByWall()
    {
        var grid = new DungeonGrid(10, 10, 32);
        var fov = new FovMap(10, 10);
        var raycaster = new Raycaster();

        // Place wall right in front of origin
        grid.SetCell(new Vector2I(5, 4), CellType.Wall);

        raycaster.ComputeFov(fov, grid, new Vector2I(5, 5), 3);

        // Wall itself is visible
        AssertInt((int)fov.GetVisibility(new Vector2I(5, 4))).IsEqual((int)VisibilityState.Visible);
        // Tile behind wall is Unexplored
        AssertInt((int)fov.GetVisibility(new Vector2I(5, 3))).IsEqual((int)VisibilityState.Unexplored);
    }
}
