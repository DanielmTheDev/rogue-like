using GdUnit4;
using Godot;
using RogueLike.Code.Entities;
using RogueLike.Code.Enemies;
using RogueLike.Code.Grid;
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
        // 5x5 grid, empty floor.
        var grid = new DungeonGrid(5, 5, 32);
        var entityManager = new EntityManager();

        // Player at (4,2)
        var player = new MockPlayer { GridPosition = new Vector2I(4, 2) };
        entityManager.RegisterActor(player);

        // Enemy at (2,2)
        var enemyActor = new MockEnemy { GridPosition = new Vector2I(2, 2) };
        entityManager.RegisterActor(enemyActor);

        var ai = new EnemyAI(enemyActor, grid, entityManager, enemyActor.GridPosition);

        // Enemy should take a step right towards (3,2)
        ai.TakeTurn();

        AssertInt(ai.GridPosition.X).IsEqual(3);
        AssertInt(ai.GridPosition.Y).IsEqual(2);
    }
}
