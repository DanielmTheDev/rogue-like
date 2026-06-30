using Xunit;
using RogueLike.Domain.Actors;
using RogueLike.Domain.Common;
using RogueLike.Domain.Combat;
using RogueLike.Domain.Grid;
using RogueLike.Domain.Grid.FOV;

namespace RogueLike.Domain.Tests.Actors;

public class EnemyAITest
{
    private class MockPlayer : ICombatant
    {
        public GridPos GridPosition { get; set; }
        public bool IsPlayer => true;
        public string DisplayName => "Mock Player";
        public Health Health { get; set; }
        public int AttackDamage => 0;
        public int XpReward => 0;
        public void Die() { }

        public void ReceiveDamage(Damage damage)
        {
            Health = Health.TakeDamage(damage.Amount);
            if (Health.IsDead) Die();
        }

        public void Heal(int amount) => Health = Health.Heal(amount);
    }

    private class MockEnemy : IActor, ICombatant
    {
        public GridPos GridPosition { get; set; }
        public bool IsPlayer => false;
        public string DisplayName => "Mock Enemy";
        public Health Health { get; set; }
        public int AttackDamage => 3;
        public int XpReward => 10;
        public void Die() { }

        public void ReceiveDamage(Damage damage)
        {
            Health = Health.TakeDamage(damage.Amount);
            if (Health.IsDead) Die();
        }

        public void Heal(int amount) => Health = Health.Heal(amount);
    }

    [Fact]
    public void TakeTurn_MovesTowardsPlayer()
    {
        var grid = new DungeonGrid(5, 5);
        var actorRegistry = new ActorRegistry();
        var fovMap = new FovMap(5, 5);

        var player = new MockPlayer { GridPosition = new GridPos(4, 2) };
        actorRegistry.RegisterActor(player);

        var enemyActor = new MockEnemy { GridPosition = new GridPos(2, 2) };
        // The mock doesn't get automatically registered by an Initialize method, so do it here.
        actorRegistry.RegisterActor(enemyActor);

        // Make the whole map visible for this test
        for (var x = 0; x < 5; x++)
            for (var y = 0; y < 5; y++)
                fovMap.SetVisibility(new GridPos(x, y), VisibilityState.Visible);

        var ai = new EnemyAI(enemyActor, grid, actorRegistry, enemyActor.GridPosition);

        // Enemy should find a path and move towards (3,2)
        ai.TakeTurn(fovMap);

        Assert.Equal(3, ai.GridPosition.X);
        Assert.Equal(2, ai.GridPosition.Y);
    }

    [Fact]
    public void TakeTurn_AdjacentToPlayer_Attacks()
    {
        var grid = new DungeonGrid(5, 5);
        var actorRegistry = new ActorRegistry();
        var fovMap = new FovMap(5, 5);

        var player = new MockPlayer { GridPosition = new GridPos(4, 2), Health = new Health(10, 10) };
        actorRegistry.RegisterActor(player);

        // Enemy adjacent to the player: its next path step is the player's (occupied) tile -> attack.
        var enemyActor = new MockEnemy { GridPosition = new GridPos(3, 2), Health = new Health(10, 10) };
        actorRegistry.RegisterActor(enemyActor);

        for (var x = 0; x < 5; x++)
            for (var y = 0; y < 5; y++)
                fovMap.SetVisibility(new GridPos(x, y), VisibilityState.Visible);

        var ai = new EnemyAI(enemyActor, grid, actorRegistry, enemyActor.GridPosition);

        ai.TakeTurn(fovMap);

        // The AI drove the enemy's own TryAttack verb: player took the enemy's damage, enemy did not move.
        Assert.Equal(7, player.Health.Current);
        Assert.Equal(3, ai.GridPosition.X);
        Assert.Equal(2, ai.GridPosition.Y);
    }

    [Fact]
    public void TakeTurn_PlayerLeftFov_MovesTowardLastKnownPosition()
    {
        var grid = new DungeonGrid(7, 7);
        var actorRegistry = new ActorRegistry();
        var fovMap = new FovMap(7, 7);

        var player = new MockPlayer { GridPosition = new GridPos(5, 2) };
        actorRegistry.RegisterActor(player);

        var enemyActor = new MockEnemy { GridPosition = new GridPos(2, 2) };
        actorRegistry.RegisterActor(enemyActor);

        var ai = new EnemyAI(enemyActor, grid, actorRegistry, enemyActor.GridPosition);

        // Turn 1: player visible -> enemy steps toward (5,2) and records it as last-known.
        fovMap.SetVisibility(player.GridPosition, VisibilityState.Visible);
        ai.TakeTurn(fovMap);
        Assert.Equal(new GridPos(3, 2), ai.GridPosition);

        // Turn 2: player tile now out of FOV -> enemy still advances toward the remembered (5,2).
        fovMap.SetVisibility(player.GridPosition, VisibilityState.Explored);
        ai.TakeTurn(fovMap);
        Assert.Equal(new GridPos(4, 2), ai.GridPosition);
    }

    [Fact]
    public void TakeTurn_ReachedLastKnownPosition_PlayerGone_Idles()
    {
        var grid = new DungeonGrid(7, 7);
        var actorRegistry = new ActorRegistry();
        var fovMap = new FovMap(7, 7);

        var player = new MockPlayer { GridPosition = new GridPos(4, 2), Health = new Health(10, 10) };
        actorRegistry.RegisterActor(player);

        var enemyActor = new MockEnemy { GridPosition = new GridPos(2, 2) };
        actorRegistry.RegisterActor(enemyActor);

        var ai = new EnemyAI(enemyActor, grid, actorRegistry, enemyActor.GridPosition);

        // See the player once at (4,2), then it slips out of FOV and relocates (freeing that tile).
        fovMap.SetVisibility(player.GridPosition, VisibilityState.Visible);
        ai.TakeTurn(fovMap); // -> (3,2)
        fovMap.SetVisibility(player.GridPosition, VisibilityState.Explored);
        var gone = new GridPos(0, 0);
        actorRegistry.UpdateActorPosition(player, player.GridPosition, gone);
        player.GridPosition = gone; // moved off, never seen again

        ai.TakeTurn(fovMap); // walks into last-known (4,2), now empty
        Assert.Equal(new GridPos(4, 2), ai.GridPosition);

        // Arrived at last-known with no player in sight -> memory cleared, now idle.
        ai.TakeTurn(fovMap);
        Assert.Equal(new GridPos(4, 2), ai.GridPosition);
    }

    [Fact]
    public void TakeTurn_NeverSawPlayer_Idles()
    {
        var grid = new DungeonGrid(5, 5);
        var actorRegistry = new ActorRegistry();
        var fovMap = new FovMap(5, 5); // nothing set Visible

        var player = new MockPlayer { GridPosition = new GridPos(4, 2) };
        actorRegistry.RegisterActor(player);

        var enemyActor = new MockEnemy { GridPosition = new GridPos(2, 2) };
        actorRegistry.RegisterActor(enemyActor);

        var ai = new EnemyAI(enemyActor, grid, actorRegistry, enemyActor.GridPosition);

        ai.TakeTurn(fovMap);

        Assert.Equal(new GridPos(2, 2), ai.GridPosition);
    }
}
