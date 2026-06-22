using GdUnit4;
using Godot;
using RogueLike.Code.Domain.Common;
using RogueLike.Code.Grid;
using RogueLike.Code.Player;
using RogueLike.Code.Entities;
using RogueLike.Code.Domain.Actors;
using static GdUnit4.Assertions;

namespace RogueLike.Tests.Player;

/// <summary>
/// Tests for GridMover — the pure movement logic.
/// No Godot runtime required.
/// </summary>
[TestSuite]
public class PlayerMovementTest
{
    private DungeonGrid CreateTestGrid()
    {
        // 5x5 grid, all floor, with a wall at (2,0)
        var grid = new DungeonGrid(5, 5);
        grid.SetCell(new Vector2I(2, 0), CellType.Wall);
        return grid;
    }

    private class MockActor : IActor
    {
        public GridPos GridPosition { get; set; }
        public bool IsPlayer { get; set; } = true;
    }

    private GridMover CreateMover(
        DungeonGrid grid, GridPos startPos, out EntityManager entityManager)
    {
        entityManager = new EntityManager();
        var actor = new MockActor { GridPosition = startPos };
        entityManager.RegisterActor(actor);
        return new GridMover(actor, grid, entityManager, startPos);
    }

    [TestCase]
    public void TryMove_ValidDirection_ReturnsTrue()
    {
        var grid = CreateTestGrid();
        var mover = CreateMover(grid, new GridPos(2, 2), out _);

        AssertBool(mover.TryMove(Direction.Right)).IsTrue();
    }

    [TestCase]
    public void TryMove_ValidDirection_UpdatesPosition()
    {
        var grid = CreateTestGrid();
        var mover = CreateMover(grid, new GridPos(2, 2), out _);

        mover.TryMove(Direction.Right);

        AssertInt(mover.GridPosition.X).IsEqual(3);
        AssertInt(mover.GridPosition.Y).IsEqual(2);
    }

    [TestCase]
    public void TryMove_IntoWall_ReturnsFalse()
    {
        var grid = CreateTestGrid();
        // Start at (2,1), wall is at (2,0)
        var mover = CreateMover(grid, new GridPos(2, 1), out _);

        // Move up into the wall
        AssertBool(mover.TryMove(Direction.Up)).IsFalse();
    }

    [TestCase]
    public void TryMove_IntoWall_PositionUnchanged()
    {
        var grid = CreateTestGrid();
        var mover = CreateMover(grid, new GridPos(2, 1), out _);

        mover.TryMove(Direction.Up);

        AssertInt(mover.GridPosition.X).IsEqual(2);
        AssertInt(mover.GridPosition.Y).IsEqual(1);
    }

    [TestCase]
    public void TryMove_OutOfBounds_ReturnsFalse()
    {
        var grid = CreateTestGrid();
        var mover = CreateMover(grid, new GridPos(0, 0), out _);

        AssertBool(mover.TryMove(Direction.Up)).IsFalse();
        AssertBool(mover.TryMove(Direction.Left)).IsFalse();
    }

    [TestCase]
    public void TryMove_OutOfBounds_PositionUnchanged()
    {
        var grid = CreateTestGrid();
        var mover = CreateMover(grid, new GridPos(0, 0), out _);

        mover.TryMove(Direction.Up);

        AssertInt(mover.GridPosition.X).IsEqual(0);
        AssertInt(mover.GridPosition.Y).IsEqual(0);
    }

    [TestCase]
    public void TryMove_MultipleSteps_TracksCorrectly()
    {
        var grid = CreateTestGrid();
        var mover = CreateMover(grid, new GridPos(2, 2), out _);

        mover.TryMove(Direction.Right); // -> (3,2)
        mover.TryMove(Direction.Down);  // -> (3,3)
        mover.TryMove(Direction.Left);  // -> (2,3)

        AssertInt(mover.GridPosition.X).IsEqual(2);
        AssertInt(mover.GridPosition.Y).IsEqual(3);
    }

    [TestCase]
    public void TryMove_DiagonalIntoOpenSpace_ReturnsTrue()
    {
        var grid = CreateTestGrid();
        var mover = CreateMover(grid, new GridPos(2, 2), out _);

        // Down-right to (3,3); both orthogonal cells (3,2) and (2,3) are floor.
        AssertBool(mover.TryMove(Direction.DownRight)).IsTrue();
        AssertInt(mover.GridPosition.X).IsEqual(3);
        AssertInt(mover.GridPosition.Y).IsEqual(3);
    }

    [TestCase]
    public void TryMove_DiagonalCornerCut_ReturnsFalse()
    {
        var grid = CreateTestGrid();
        grid.SetCell(new Vector2I(3, 2), CellType.Wall); // block one orthogonal side
        var mover = CreateMover(grid, new GridPos(2, 2), out _);

        // Down-right target (3,3) is floor, but it would cut the corner past wall (3,2).
        AssertBool(mover.TryMove(Direction.DownRight)).IsFalse();
        AssertInt(mover.GridPosition.X).IsEqual(2);
        AssertInt(mover.GridPosition.Y).IsEqual(2);
    }

    [TestCase]
    public void Initialize_SetsCorrectPosition()
    {
        var grid = CreateTestGrid();
        var mover = CreateMover(grid, new GridPos(3, 4), out _);

        AssertInt(mover.GridPosition.X).IsEqual(3);
        AssertInt(mover.GridPosition.Y).IsEqual(4);
    }
}
