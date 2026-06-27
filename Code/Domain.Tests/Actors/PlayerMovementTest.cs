using Xunit;
using RogueLike.Domain.Common;
using RogueLike.Domain.Grid;
using RogueLike.Domain.Actors;

namespace RogueLike.Domain.Tests.Actors;

/// <summary>
/// Tests for GridMover — the pure movement logic.
/// No Godot runtime required.
/// </summary>
public class PlayerMovementTest
{
    private DungeonGrid CreateTestGrid()
    {
        // 5x5 grid, all floor, with a wall at (2,0)
        var grid = new DungeonGrid(5, 5);
        grid.SetCell(new GridPos(2, 0), CellType.Wall);
        return grid;
    }

    private class MockActor : IActor
    {
        public GridPos GridPosition { get; set; }
        public bool IsPlayer { get; set; } = true;
    }

    private GridMover CreateMover(
        DungeonGrid grid, GridPos startPos, out ActorRegistry actorRegistry)
    {
        actorRegistry = new ActorRegistry();
        var actor = new MockActor { GridPosition = startPos };
        actorRegistry.RegisterActor(actor);
        return new GridMover(actor, grid, actorRegistry, startPos);
    }

    [Fact]
    public void TryMove_ValidDirection_ReturnsTrue()
    {
        var grid = CreateTestGrid();
        var mover = CreateMover(grid, new GridPos(2, 2), out _);

        Assert.True(mover.TryMove(Direction.Right));
    }

    [Fact]
    public void TryMove_ValidDirection_UpdatesPosition()
    {
        var grid = CreateTestGrid();
        var mover = CreateMover(grid, new GridPos(2, 2), out _);

        mover.TryMove(Direction.Right);

        Assert.Equal(3, mover.GridPosition.X);
        Assert.Equal(2, mover.GridPosition.Y);
    }

    [Fact]
    public void TryMove_IntoWall_ReturnsFalse()
    {
        var grid = CreateTestGrid();
        // Start at (2,1), wall is at (2,0)
        var mover = CreateMover(grid, new GridPos(2, 1), out _);

        // Move up into the wall
        Assert.False(mover.TryMove(Direction.Up));
    }

    [Fact]
    public void TryMove_IntoWall_PositionUnchanged()
    {
        var grid = CreateTestGrid();
        var mover = CreateMover(grid, new GridPos(2, 1), out _);

        mover.TryMove(Direction.Up);

        Assert.Equal(2, mover.GridPosition.X);
        Assert.Equal(1, mover.GridPosition.Y);
    }

    [Fact]
    public void TryMove_OutOfBounds_ReturnsFalse()
    {
        var grid = CreateTestGrid();
        var mover = CreateMover(grid, new GridPos(0, 0), out _);

        Assert.False(mover.TryMove(Direction.Up));
        Assert.False(mover.TryMove(Direction.Left));
    }

    [Fact]
    public void TryMove_OutOfBounds_PositionUnchanged()
    {
        var grid = CreateTestGrid();
        var mover = CreateMover(grid, new GridPos(0, 0), out _);

        mover.TryMove(Direction.Up);

        Assert.Equal(0, mover.GridPosition.X);
        Assert.Equal(0, mover.GridPosition.Y);
    }

    [Fact]
    public void TryMove_MultipleSteps_TracksCorrectly()
    {
        var grid = CreateTestGrid();
        var mover = CreateMover(grid, new GridPos(2, 2), out _);

        mover.TryMove(Direction.Right); // -> (3,2)
        mover.TryMove(Direction.Down);  // -> (3,3)
        mover.TryMove(Direction.Left);  // -> (2,3)

        Assert.Equal(2, mover.GridPosition.X);
        Assert.Equal(3, mover.GridPosition.Y);
    }

    [Fact]
    public void TryMove_DiagonalIntoOpenSpace_ReturnsTrue()
    {
        var grid = CreateTestGrid();
        var mover = CreateMover(grid, new GridPos(2, 2), out _);

        // Down-right to (3,3); both orthogonal cells (3,2) and (2,3) are floor.
        Assert.True(mover.TryMove(Direction.DownRight));
        Assert.Equal(3, mover.GridPosition.X);
        Assert.Equal(3, mover.GridPosition.Y);
    }

    [Fact]
    public void TryMove_DiagonalPastWallCorner_Succeeds()
    {
        var grid = CreateTestGrid();
        grid.SetCell(new GridPos(3, 2), CellType.Wall); // wall on one orthogonal side
        var mover = CreateMover(grid, new GridPos(2, 2), out _);

        // Down-right target (3,3) is floor; rounding the corner past wall (3,2) is allowed.
        Assert.True(mover.TryMove(Direction.DownRight));
        Assert.Equal(3, mover.GridPosition.X);
        Assert.Equal(3, mover.GridPosition.Y);
    }

    [Fact]
    public void TryMove_DiagonalIntoWall_ReturnsFalse()
    {
        var grid = CreateTestGrid();
        grid.SetCell(new GridPos(3, 3), CellType.Wall); // the diagonal target itself is a wall
        var mover = CreateMover(grid, new GridPos(2, 2), out _);

        Assert.False(mover.TryMove(Direction.DownRight));
        Assert.Equal(2, mover.GridPosition.X);
        Assert.Equal(2, mover.GridPosition.Y);
    }

    [Fact]
    public void Initialize_SetsCorrectPosition()
    {
        var grid = CreateTestGrid();
        var mover = CreateMover(grid, new GridPos(3, 4), out _);

        Assert.Equal(3, mover.GridPosition.X);
        Assert.Equal(4, mover.GridPosition.Y);
    }
}
