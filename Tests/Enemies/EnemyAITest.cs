using GdUnit4;
using Godot;
using RogueLike.Code.Entities;
using RogueLike.Code.Entities.Combat;
using RogueLike.Code.Enemies;
using RogueLike.Code.Grid;
using RogueLike.Code.Grid.FOV;
using RogueLike.Code.Pathfinding;
using static GdUnit4.Assertions;

namespace RogueLike.Tests.Enemies;

[TestSuite]
public class EnemyAITest
{
    private class MockPlayer : IActor, ICombatant
    {
        public Vector2I GridPosition { get; set; }
        public bool IsPlayer => true;
        public string DisplayName => "Mock Player";
        public HealthController Health { get; set; }
        public int AttackDamage => 0;
        public int XpReward => 0;
        public void Die() { }
    }

    private class MockEnemy : IActor, ICombatant
    {
        public Vector2I GridPosition { get; set; }
        public bool IsPlayer => false;
        public string DisplayName => "Mock Enemy";
        public HealthController Health { get; set; }
        public int AttackDamage => 3;
        public int XpReward => 10;
        public void Die() { }
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

    [TestCase]
    public void TakeTurn_AdjacentToPlayer_Attacks()
    {
        var grid = new DungeonGrid(5, 5);
        var entityManager = new EntityManager();
        var pathfinder = new Pathfinder();
        var fovMap = new FovMap(5, 5);

        var player = new MockPlayer { GridPosition = new Vector2I(4, 2), Health = new HealthController(10) };
        entityManager.RegisterActor(player);

        // Enemy adjacent to the player: its next path step is the player's (occupied) tile -> attack.
        var enemyActor = new MockEnemy { GridPosition = new Vector2I(3, 2), Health = new HealthController(10) };
        entityManager.RegisterActor(enemyActor);

        for (var x = 0; x < 5; x++)
        for (var y = 0; y < 5; y++)
            fovMap.SetVisibility(new Vector2I(x, y), VisibilityState.Visible);

        var ai = new EnemyAI(enemyActor, grid, entityManager, pathfinder, enemyActor.GridPosition);

        ai.TakeTurn(fovMap);

        // The AI drove the enemy's own TryAttack verb: player took the enemy's damage, enemy did not move.
        AssertInt(player.Health.CurrentHp).IsEqual(7);
        AssertInt(ai.GridPosition.X).IsEqual(3);
        AssertInt(ai.GridPosition.Y).IsEqual(2);
    }
}
