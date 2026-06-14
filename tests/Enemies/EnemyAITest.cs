using GdUnit4;
using Godot;
using RogueLike.Code.Entities;
using RogueLike.Code.Enemies;
using RogueLike.Code.Grid;
using RogueLike.Code.Grid.FOV;
using RogueLike.Code.Pathfinding;
using static GdUnit4.Assertions;

namespace RogueLike.tests.Enemies;

[TestSuite]
public class EnemyAITest
{
    private class MockPlayer : IActor
    {
        public Vector2I GridPosition { get; set; }
        public bool IsPlayer => true;
    }

    private class MockEnemy : IActor
    {
        public Vector2I GridPosition { get; set; }
        public bool IsPlayer => false;
    }

    [TestCase]
    public void TakeTurn_MovesTowardsPlayer()
    {
        var grid = new DungeonGrid(5, 5);
        var entityManager = new EntityManager();
        var pathfinder = new Pathfinder();
        var fovMap = new FovMap(5, 5);

        var player = new MockPlayer { GridPosition = new Vector2I(4, 2) };
        entityManager.RegisterActor(player);

        var enemyActor = new MockEnemy { GridPosition = new Vector2I(2, 2) };
        // The mock doesn't get automatically registered by an Initialize method, so do it here.
        entityManager.RegisterActor(enemyActor);

        // Make the whole map visible for this test
        for(var x = 0; x < 5; x++)
        for(var y = 0; y < 5; y++)
            fovMap.SetVisibility(new Vector2I(x, y), VisibilityState.Visible);

        var ai = new EnemyAI(enemyActor, grid, entityManager, pathfinder, enemyActor.GridPosition);

        // Enemy should find a path and move towards (3,2)
        ai.TakeTurn(fovMap);

        AssertInt(ai.GridPosition.X).IsEqual(3);
        AssertInt(ai.GridPosition.Y).IsEqual(2);
    }
}
