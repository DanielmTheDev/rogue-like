using GdUnit4;
using Godot;
using RogueLike.Code.Grid;
using RogueLike.Code.Player;
using RogueLike.Code.Entities;
using static GdUnit4.Assertions;

namespace RogueLike.tests.Player;

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
        var grid = new DungeonGrid(5, 5, 32);
        grid.SetCell(new Vector2I(2, 0), CellType.Wall);
        return grid;
    }

    private class MockActor : IActor
    {
        public Vector2I GridPosition { get; set; }
        public bool IsPlayer { get; set; } = true;
    }

    private GridMover CreateMover(
        DungeonGrid grid, Vector2I startPos, out EntityManager entityManager)
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
        var mover = CreateMover(grid, new Vector2I(2, 2), out _);

        AssertBool(mover.TryMove(Vector2I.Right)).IsTrue();
    }

    [TestCase]
    public void TryMove_ValidDirection_UpdatesPosition()
    {
        var grid = CreateTestGrid();
        var mover = CreateMover(grid, new Vector2I(2, 2), out _);

        mover.TryMove(Vector2I.Right);

        AssertInt(mover.GridPosition.X).IsEqual(3);
        AssertInt(mover.GridPosition.Y).IsEqual(2);
    }

    [TestCase]
    public void TryMove_IntoWall_ReturnsFalse()
    {
        var grid = CreateTestGrid();
        // Start at (2,1), wall is at (2,0)
        var mover = CreateMover(grid, new Vector2I(2, 1), out _);

        // Move up into the wall
        AssertBool(mover.TryMove(Vector2I.Up)).IsFalse();
    }

    [TestCase]
    public void TryMove_IntoWall_PositionUnchanged()
    {
        var grid = CreateTestGrid();
        var mover = CreateMover(grid, new Vector2I(2, 1), out _);

        mover.TryMove(Vector2I.Up);

        AssertInt(mover.GridPosition.X).IsEqual(2);
        AssertInt(mover.GridPosition.Y).IsEqual(1);
    }

    [TestCase]
    public void TryMove_OutOfBounds_ReturnsFalse()
    {
        var grid = CreateTestGrid();
        var mover = CreateMover(grid, new Vector2I(0, 0), out _);

        AssertBool(mover.TryMove(Vector2I.Up)).IsFalse();
        AssertBool(mover.TryMove(Vector2I.Left)).IsFalse();
    }

    [TestCase]
    public void TryMove_OutOfBounds_PositionUnchanged()
    {
        var grid = CreateTestGrid();
        var mover = CreateMover(grid, new Vector2I(0, 0), out _);

        mover.TryMove(Vector2I.Up);

        AssertInt(mover.GridPosition.X).IsEqual(0);
        AssertInt(mover.GridPosition.Y).IsEqual(0);
    }

    [TestCase]
    public void TryMove_MultipleSteps_TracksCorrectly()
    {
        var grid = CreateTestGrid();
        var mover = CreateMover(grid, new Vector2I(2, 2), out _);

        mover.TryMove(Vector2I.Right); // -> (3,2)
        mover.TryMove(Vector2I.Down);  // -> (3,3)
        mover.TryMove(Vector2I.Left);  // -> (2,3)

        AssertInt(mover.GridPosition.X).IsEqual(2);
        AssertInt(mover.GridPosition.Y).IsEqual(3);
    }

    [TestCase]
    public void Initialize_SetsCorrectPosition()
    {
        var grid = CreateTestGrid();
        var mover = CreateMover(grid, new Vector2I(3, 4), out _);

        AssertInt(mover.GridPosition.X).IsEqual(3);
        AssertInt(mover.GridPosition.Y).IsEqual(4);
    }
}
