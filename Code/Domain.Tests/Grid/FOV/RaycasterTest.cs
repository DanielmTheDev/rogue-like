using Xunit;
using RogueLike.Domain.Common;
using RogueLike.Domain.Grid;
using RogueLike.Domain.Grid.FOV;

namespace RogueLike.Domain.Tests.Grid.FOV;

public class RaycasterTest
{
    [Fact]
    public void ComputeFov_ClearsPreviousVisibleTiles()
    {
        var grid = new DungeonGrid(10, 10);
        var fov = new FovMap(10, 10);
        var raycaster = new Raycaster();

        // compute first time
        raycaster.ComputeFov(fov, grid, new GridPos(5, 5), 3);
        Assert.Equal(VisibilityState.Visible, fov.GetVisibility(new GridPos(5, 6)));

        // move away
        raycaster.ComputeFov(fov, grid, new GridPos(1, 1), 1);

        // previous tiles should now be Explored, not Visible
        Assert.Equal(VisibilityState.Explored, fov.GetVisibility(new GridPos(5, 6)));
    }

    [Fact]
    public void ComputeFov_BlockedByWall()
    {
        var grid = new DungeonGrid(10, 10);
        var fov = new FovMap(10, 10);
        var raycaster = new Raycaster();

        // Place wall right in front of origin
        grid.SetCell(new GridPos(5, 4), CellType.Wall);

        raycaster.ComputeFov(fov, grid, new GridPos(5, 5), 3);

        // Wall itself is visible
        Assert.Equal(VisibilityState.Visible, fov.GetVisibility(new GridPos(5, 4)));
        // Tile behind wall is Unexplored
        Assert.Equal(VisibilityState.Unexplored, fov.GetVisibility(new GridPos(5, 3)));
    }
}
