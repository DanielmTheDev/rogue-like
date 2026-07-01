using Xunit;
using RogueLike.Domain.Actors;
using RogueLike.Domain.Grid;
using RogueLike.Domain.Grid.FOV;
using RogueLike.Domain.Combat;
using RogueLike.Domain.Common;

namespace RogueLike.Domain.Tests.Actors;

public class ArcherAITest
{
    private class MockPlayer : ICombatant
    {
        public GridPos GridPosition { get; set; }
        public bool IsPlayer => true;
        public Health Health { get; set; }
        public int AttackDamage => 0;
        public string DisplayName => "Mock Player";
        public int XpReward => 0;
        public void Die() { }

        public void ReceiveDamage(Damage damage)
        {
            Health = Health.TakeDamage(damage.Amount);
            if (Health.IsDead) Die();
        }

        public void Heal(int amount) => Health = Health.Heal(amount);
    }

    private class MockArcher : ICombatant
    {
        public GridPos GridPosition { get; set; }
        public bool IsPlayer => false;
        public int AttackDamage => 1;
        public string DisplayName => "Mock Archer";
        public Health Health { get; set; }
        public int XpReward => 10;
        public void Die() { }

        public void ReceiveDamage(Damage damage)
        {
            Health = Health.TakeDamage(damage.Amount);
            if (Health.IsDead) Die();
        }

        public void Heal(int amount) => Health = Health.Heal(amount);
    }

    private readonly DungeonGrid _grid;
    private readonly ActorRegistry _actorRegistry;
    private readonly FovMap _fovMap;

    public ArcherAITest()
    {
        _grid = new DungeonGrid(10, 10);
        _actorRegistry = new ActorRegistry();
        _fovMap = new FovMap(10, 10);
    }

    [Fact]
    public void TakeTurn_DoesNotShootThroughWalls()
    {
        // Arrange
        var player = new MockPlayer();
        var archer = new MockArcher();

        // Place player and archer with a wall between them
        player.GridPosition = new GridPos(1, 1);
        archer.GridPosition = new GridPos(1, 3);
        _grid.SetCell(new GridPos(1, 2), CellType.Wall);

        // Register actors with ActorRegistry
        _actorRegistry.RegisterActor(player);
        _actorRegistry.RegisterActor(archer);

        // Even if the player is "visible" in the FOV map, the archer should not shoot.
        _fovMap.SetVisibility(player.GridPosition, VisibilityState.Visible);

        // Initialize health
        player.Health = new Health(10, 10);
        archer.Health = new Health(10, 10);

        // The player should NOT have taken damage because there is a wall in the way.
        var ai = new ArcherAI(archer, _grid, _actorRegistry, archer.GridPosition);

        // Act
        ai.TakeTurn(_fovMap);

        // Assert
        Assert.Equal(10, player.Health.Current);
    }

    [Fact]
    public void TakeTurn_InRangeWithClearLos_Shoots()
    {
        var player = new MockPlayer { GridPosition = new GridPos(1, 1), Health = new Health(10, 10) };
        var archer = new MockArcher { GridPosition = new GridPos(1, 3), Health = new Health(10, 10) };

        _actorRegistry.RegisterActor(player);
        _actorRegistry.RegisterActor(archer);
        _fovMap.SetVisibility(player.GridPosition, VisibilityState.Visible);

        var ai = new ArcherAI(archer, _grid, _actorRegistry, archer.GridPosition);

        // Distance 2, no wall, within range -> the AI drives the archer's TryAttack verb.
        ai.TakeTurn(_fovMap);

        Assert.Equal(9, player.Health.Current); // archer AttackDamage == 1
    }

    [Fact]
    public void TakeTurn_PlayerLeftFov_MovesTowardLastKnownPosition()
    {
        var player = new MockPlayer { GridPosition = new GridPos(1, 1), Health = new Health(10, 10) };
        var archer = new MockArcher { GridPosition = new GridPos(1, 8), Health = new Health(10, 10) };

        _actorRegistry.RegisterActor(player);
        _actorRegistry.RegisterActor(archer);

        var ai = new ArcherAI(archer, _grid, _actorRegistry, archer.GridPosition);

        // Turn 1: visible but out of range (Manhattan 7 > 5) -> close the distance, record last-known.
        _fovMap.SetVisibility(player.GridPosition, VisibilityState.Visible);
        ai.TakeTurn(_fovMap);
        Assert.Equal(new GridPos(1, 7), ai.GridPosition);

        // Turn 2: player out of FOV -> keep advancing toward the remembered tile. No blind-fire.
        _fovMap.SetVisibility(player.GridPosition, VisibilityState.Explored);
        ai.TakeTurn(_fovMap);
        Assert.Equal(new GridPos(1, 6), ai.GridPosition);
        Assert.Equal(10, player.Health.Current);
    }

    [Fact]
    public void TakeTurn_ReachedLastKnown_PlayerGone_Idles()
    {
        var player = new MockPlayer { GridPosition = new GridPos(1, 5), Health = new Health(10, 10) };
        var archer = new MockArcher { GridPosition = new GridPos(1, 6), Health = new Health(10, 10) };

        _actorRegistry.RegisterActor(player);
        _actorRegistry.RegisterActor(archer);

        var ai = new ArcherAI(archer, _grid, _actorRegistry, archer.GridPosition);

        // Turn 1: in range + clear line -> shoot, record last-known (1,5), no move.
        _fovMap.SetVisibility(player.GridPosition, VisibilityState.Visible);
        ai.TakeTurn(_fovMap);
        Assert.Equal(new GridPos(1, 6), ai.GridPosition);

        // Player slips away (frees its tile) and stays out of FOV.
        _fovMap.SetVisibility(player.GridPosition, VisibilityState.Explored);
        var gone = new GridPos(9, 9);
        _actorRegistry.UpdateActorPosition(player, player.GridPosition, gone);
        player.GridPosition = gone;

        ai.TakeTurn(_fovMap); // step into last-known (1,5)
        Assert.Equal(new GridPos(1, 5), ai.GridPosition);

        ai.TakeTurn(_fovMap); // arrived, nobody there -> forget + idle
        Assert.Equal(new GridPos(1, 5), ai.GridPosition);
    }
}
