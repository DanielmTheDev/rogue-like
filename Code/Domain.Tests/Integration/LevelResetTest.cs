using Xunit;
using RogueLike.Domain.Actors;
using RogueLike.Domain.Common;
using RogueLike.Domain.Grid;
using RogueLike.Domain.Items;

namespace RogueLike.Domain.Tests.Integration;

/// <summary>
/// Godot-free integration test for the domain-side reset contract that
/// <c>Main.DescendLevel</c> depends on: clearing the actor and floor-item registries must leave
/// no residual state, so a freshly generated level starts from a clean slate. Mirrors the
/// registry-clearing the view orchestrator performs, without Godot.
/// </summary>
public class LevelResetTest
{
    [Fact]
    public void Reset_EmptiesActorRegistryAndFloorItems()
    {
        var grid = new DungeonGrid(20, 20);
        var registry = new ActorRegistry();
        var floor = new FloorItems();
        PopulateLevel(grid, registry, floor);

        Assert.NotEmpty(registry.AllActors);
        Assert.NotEmpty(floor.AllItems);

        // What DescendLevel does domain-side when leaving a level.
        registry.ClearAll();
        floor.Clear();

        Assert.Empty(registry.AllActors);
        Assert.Empty(floor.AllItems);
        Assert.False(registry.IsOccupied(new GridPos(2, 2)));
        Assert.False(registry.IsOccupied(new GridPos(5, 5)));
    }

    [Fact]
    public void AfterReset_PreviouslyOccupiedTile_CanBeReoccupied()
    {
        var grid = new DungeonGrid(20, 20);
        var registry = new ActorRegistry();
        var floor = new FloorItems();
        PopulateLevel(grid, registry, floor);
        var oldPlayerTile = new GridPos(2, 2);

        registry.ClearAll();

        // No stale occupancy entry must block a respawn on the old tile.
        var newPlayer = new PlayerDouble(oldPlayerTile);
        registry.RegisterActor(newPlayer);

        Assert.True(registry.IsOccupied(oldPlayerTile));
        Assert.Same(newPlayer, registry.GetActorAt(oldPlayerTile));
        Assert.Single(registry.AllActors);
    }

    private static void PopulateLevel(DungeonGrid grid, ActorRegistry registry, FloorItems floor)
    {
        var player = new PlayerDouble(new GridPos(2, 2));
        registry.RegisterActor(player);

        var enemy = new EnemyDouble(new GridPos(5, 5));
        enemy.Ai = new EnemyAI(enemy, grid, registry, new GridPos(5, 5), 7);
        registry.RegisterActor(enemy);

        floor.RegisterItem(new ItemDouble(new GridPos(3, 3)));
    }
}
