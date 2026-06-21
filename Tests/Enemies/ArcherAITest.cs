using GdUnit4;
using Godot;
using RogueLike.Code.Entities;
using RogueLike.Code.Enemies;
using RogueLike.Code.Grid;
using RogueLike.Code.Grid.FOV;
using RogueLike.Code.Entities.Combat;
using RogueLike.Code.Domain.Common;
using static GdUnit4.Assertions;

namespace RogueLike.Tests.Enemies;

[TestSuite]
public class ArcherAITest
{
    private class MockPlayer : IActor, ICombatant
    {
        public GridPos GridPosition { get; set; }
        public bool IsPlayer => true;
        public HealthController Health { get; set; }
        public int AttackDamage => 0;
        public string DisplayName => "Mock Player";
        public int XpReward => 0;
        public void Die() { }
    }

    private class MockArcher : IActor, ICombatant
    {
        public GridPos GridPosition { get; set; }
        public bool IsPlayer => false;
        public int AttackDamage => 1;
        public string DisplayName => "Mock Archer";
        public HealthController Health { get; set; }
        public int XpReward => 10;
        public void Die() { }
    }

    private DungeonGrid _grid;
    private EntityManager _entityManager;
    private FovMap _fovMap;

    [BeforeTest]
    public void Setup()
    {
        _grid = new DungeonGrid(10, 10);
        _entityManager = new EntityManager();
        _fovMap = new FovMap(10, 10);
    }

    [TestCase]
    public void TakeTurn_DoesNotShootThroughWalls()
    {
        // Arrange
        var player = new MockPlayer();
        var archer = new MockArcher();

        // Place player and archer with a wall between them
        player.GridPosition = new GridPos(1, 1);
        archer.GridPosition = new GridPos(1, 3);
        _grid.SetCell(new Vector2I(1, 2), CellType.Wall);

        // Register actors with EntityManager
        _entityManager.RegisterActor(player);
        _entityManager.RegisterActor(archer);

        // Even if the player is "visible" in the FOV map, the archer should not shoot.
        _fovMap.SetVisibility(player.GridPosition, VisibilityState.Visible);

        // Initialize health
        player.Health = new HealthController(10);
        archer.Health = new HealthController(10);

        // The player should NOT have taken damage because there is a wall in the way.
        var ai = new ArcherAI(archer, _grid, _entityManager, archer.GridPosition);

        // Act
        ai.TakeTurn(_fovMap);

        // Assert
        AssertThat(player.Health.CurrentHp).IsEqual(10);
    }

    [TestCase]
    public void TakeTurn_InRangeWithClearLos_Shoots()
    {
        var player = new MockPlayer { GridPosition = new GridPos(1, 1), Health = new HealthController(10) };
        var archer = new MockArcher { GridPosition = new GridPos(1, 3), Health = new HealthController(10) };

        _entityManager.RegisterActor(player);
        _entityManager.RegisterActor(archer);
        _fovMap.SetVisibility(player.GridPosition, VisibilityState.Visible);

        var ai = new ArcherAI(archer, _grid, _entityManager, archer.GridPosition);

        // Distance 2, no wall, within range -> the AI drives the archer's TryAttack verb.
        ai.TakeTurn(_fovMap);

        AssertThat(player.Health.CurrentHp).IsEqual(9); // archer AttackDamage == 1
    }
}
