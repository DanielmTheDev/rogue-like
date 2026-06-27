using Xunit;
using RogueLike.Domain.Actors;
using RogueLike.Domain.Combat;
using RogueLike.Domain.Common;
using RogueLike.Domain.Flow;
using RogueLike.Domain.Grid;
using RogueLike.Domain.Grid.FOV;

namespace RogueLike.Domain.Tests.Integration;

/// <summary>
/// Godot-free integration test: wires the real domain subsystems together
/// (TurnManager + ActorRegistry + EnemyAI + GridMover + DungeonGrid pathfinding + Combat)
/// and drives a Player→Enemy→Player turn cycle. Uses lightweight domain doubles that
/// implement the production <see cref="ICombatant"/> interface — no controllers, no Godot.
/// This is the middle of the test pyramid: catches cross-system wiring breaks that
/// per-class unit tests cannot, at near-unit speed.
/// </summary>
public class TurnCycleTest
{
    [Fact]
    public void Enemy_PursuesPlayer_AcrossOneTurnCycle()
    {
        var s = BuildScenario(playerPos: new GridPos(2, 10), enemyPos: new GridPos(8, 10));
        Assert.Equal(TurnState.Player, s.Turn.CurrentState);
        var distBefore = s.Enemy.GridPosition.ManhattanTo(s.Player.GridPosition);

        // One full cycle: player acts, enemy reacts, control returns to player.
        s.Turn.EndPlayerTurn();
        Assert.Equal(TurnState.Enemy, s.Turn.CurrentState);
        s.Enemy.Ai.TakeTurn(s.Fov);
        s.Turn.EndEnemyTurn();

        Assert.Equal(TurnState.Player, s.Turn.CurrentState);
        var distAfter = s.Enemy.GridPosition.ManhattanTo(s.Player.GridPosition);
        Assert.True(distAfter < distBefore, $"enemy should close in on player: {distBefore} -> {distAfter}");

        // ActorRegistry must track the enemy's NEW tile (occupancy stays consistent).
        Assert.True(s.Registry.IsOccupied(s.Enemy.GridPosition));
        Assert.Same(s.Enemy, s.Registry.GetActorAt(s.Enemy.GridPosition));
    }

    [Fact]
    public void Enemy_AttacksPlayer_WhenAdjacent_ReducingHealthWithoutMoving()
    {
        var s = BuildScenario(playerPos: new GridPos(2, 10), enemyPos: new GridPos(3, 10));
        var hpBefore = s.Player.Health.Current;

        s.Turn.EndPlayerTurn();
        s.Enemy.Ai.TakeTurn(s.Fov); // player is on the adjacent tile -> attack, not move
        s.Turn.EndEnemyTurn();

        Assert.True(s.Player.Health.Current < hpBefore, "adjacent enemy should damage the player");
        Assert.Equal(new GridPos(3, 10), s.Enemy.GridPosition); // attacked in place, did not step
    }

    private static Scenario BuildScenario(GridPos playerPos, GridPos enemyPos)
    {
        var grid = new DungeonGrid(20, 20); // ctor fills with Floor -> open room, deterministic A*
        var registry = new ActorRegistry();
        var fov = new FovMap(20, 20);
        var turn = new TurnManager();

        var player = new PlayerDouble(playerPos);
        registry.RegisterActor(player);

        var enemy = new EnemyDouble(enemyPos);
        enemy.Ai = new EnemyAI(enemy, grid, registry, enemyPos);
        registry.RegisterActor(enemy);

        // Player-centred FOV; EnemyAI only acts while the player's tile is Visible.
        new Raycaster().ComputeFov(fov, grid, playerPos, 20);

        return new Scenario(turn, registry, fov, grid, player, enemy);
    }

    private sealed record Scenario(
        TurnManager Turn,
        ActorRegistry Registry,
        FovMap Fov,
        DungeonGrid Grid,
        PlayerDouble Player,
        EnemyDouble Enemy);
}
