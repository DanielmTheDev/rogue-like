using Xunit;
using RogueLike.Domain.Common;
using RogueLike.Domain.Grid;

namespace RogueLike.Domain.Tests.Grid;

public class DungeonGridTest
{
    [Fact]
    public void IsInBounds_InsideGrid_ReturnsTrue()
    {
        var grid = new DungeonGrid(10, 8);

        Assert.True(grid.IsInBounds(new GridPos(0, 0)));
        Assert.True(grid.IsInBounds(new GridPos(5, 4)));
        Assert.True(grid.IsInBounds(new GridPos(9, 7)));
    }

    [Fact]
    public void IsInBounds_OutsideGrid_ReturnsFalse()
    {
        var grid = new DungeonGrid(10, 8);

        Assert.False(grid.IsInBounds(new GridPos(-1, 0)));
        Assert.False(grid.IsInBounds(new GridPos(0, -1)));
        Assert.False(grid.IsInBounds(new GridPos(10, 0)));
        Assert.False(grid.IsInBounds(new GridPos(0, 8)));
    }

    [Fact]
    public void IsWalkable_FloorCell_ReturnsTrue()
    {
        var grid = new DungeonGrid(10, 8);

        // Default fill is Floor
        Assert.True(grid.IsWalkable(new GridPos(3, 3)));
    }

    [Fact]
    public void IsWalkable_WallCell_ReturnsFalse()
    {
        var grid = new DungeonGrid(10, 8);
        grid.SetCell(new GridPos(3, 3), CellType.Wall);

        Assert.False(grid.IsWalkable(new GridPos(3, 3)));
    }

    [Fact]
    public void IsWalkable_OutOfBounds_ReturnsFalse()
    {
        var grid = new DungeonGrid(10, 8);

        Assert.False(grid.IsWalkable(new GridPos(-1, 0)));
        Assert.False(grid.IsWalkable(new GridPos(10, 0)));
    }

    [Fact]
    public void GetCell_OutOfBounds_ReturnsWall()
    {
        var grid = new DungeonGrid(10, 8);

        Assert.Equal(CellType.Wall, grid.GetCell(new GridPos(-1, 0)));
    }

    [Fact]
    public void SetCell_ThenGetCell_ReturnsCorrectType()
    {
        var grid = new DungeonGrid(10, 8);
        grid.SetCell(new GridPos(2, 3), CellType.Wall);

        Assert.Equal(CellType.Wall, grid.GetCell(new GridPos(2, 3)));
    }
}
