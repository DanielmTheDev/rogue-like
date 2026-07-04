using Xunit;
using RogueLike.Domain.Actors;
using RogueLike.Domain.Grid;
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

    public ArcherAITest()
    {
        _grid = new DungeonGrid(10, 10);
        _actorRegistry = new ActorRegistry();
    }

    [Fact]
    public void TakeTurn_DoesNotShootThroughWalls()
    {
        var player = new MockPlayer { GridPosition = new GridPos(1, 1), Health = new Health(10, 10) };
        var archer = new MockArcher { GridPosition = new GridPos(1, 3), Health = new Health(10, 10) };

        // Wall between them: line-of-sight is blocked, so the archer cannot even see (let alone shoot).
        _grid.SetCell(new GridPos(1, 2), CellType.Wall);
        _actorRegistry.RegisterActor(player);
        _actorRegistry.RegisterActor(archer);

        var ai = new ArcherAI(archer, _grid, _actorRegistry, archer.GridPosition, range: 5, sightRange: 7);

        ai.TakeTurn();

        Assert.Equal(10, player.Health.Current);
    }

    [Fact]
    public void TakeTurn_InRangeWithClearLos_Shoots()
    {
        var player = new MockPlayer { GridPosition = new GridPos(1, 1), Health = new Health(10, 10) };
        var archer = new MockArcher { GridPosition = new GridPos(1, 3), Health = new Health(10, 10) };

        _actorRegistry.RegisterActor(player);
        _actorRegistry.RegisterActor(archer);

        var ai = new ArcherAI(archer, _grid, _actorRegistry, archer.GridPosition, range: 5, sightRange: 7);

        // Distance 2, clear line, within shoot range -> the AI drives the archer's TryAttack verb.
        ai.TakeTurn();

        Assert.Equal(9, player.Health.Current); // archer AttackDamage == 1
    }

    [Fact]
    public void TakeTurn_PlayerLeftSight_MovesTowardLastKnownPosition()
    {
        var player = new MockPlayer { GridPosition = new GridPos(1, 1), Health = new Health(10, 10) };
        var archer = new MockArcher { GridPosition = new GridPos(1, 8), Health = new Health(10, 10) };

        _actorRegistry.RegisterActor(player);
        _actorRegistry.RegisterActor(archer);

        var ai = new ArcherAI(archer, _grid, _actorRegistry, archer.GridPosition, range: 5, sightRange: 7);

        // Turn 1: in sight (Chebyshev 7) but out of shoot range (Manhattan 7 > 5) -> close in, record last-known.
        ai.TakeTurn();
        Assert.Equal(new GridPos(1, 7), ai.GridPosition);

        // Turn 2: player slips far out of sight -> keep advancing toward remembered tile. No blind-fire.
        var gone = new GridPos(9, 9);
        _actorRegistry.UpdateActorPosition(player, player.GridPosition, gone);
        player.GridPosition = gone;
        ai.TakeTurn();
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

        var ai = new ArcherAI(archer, _grid, _actorRegistry, archer.GridPosition, range: 5, sightRange: 7);

        // Turn 1: in sight + shoot range -> shoot, record last-known (1,5), no move.
        ai.TakeTurn();
        Assert.Equal(new GridPos(1, 6), ai.GridPosition);

        // Player slips far away (frees its tile) and out of sight.
        var gone = new GridPos(9, 9);
        _actorRegistry.UpdateActorPosition(player, player.GridPosition, gone);
        player.GridPosition = gone;

        ai.TakeTurn(); // step into last-known (1,5)
        Assert.Equal(new GridPos(1, 5), ai.GridPosition);

        ai.TakeTurn(); // arrived, nobody there -> forget + idle
        Assert.Equal(new GridPos(1, 5), ai.GridPosition);
    }
}
