using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GdUnit4;
using Godot;
using RogueLike.Code.View.Enemies;
using RogueLike.Code.View.Entities;
using RogueLike.Code.View.Grid;
using RogueLike.Code.View.Items;
using RogueLike.Code.View.Player;
using RogueLike.Code.View.World;
using RogueLike.Domain.Common;
using static GdUnit4.Assertions;

namespace RogueLike.Code.View.Tests;

/// <summary>
/// Top-of-pyramid scene tests: boot the REAL production scene (Scenes/Main.tscn) under headless
/// Godot via gdUnit4's ISceneRunner and assert behaviour through the live node tree. Catches
/// whole-system wiring breaks the Godot-free domain/integration tests structurally cannot — scene
/// paths, GetNode lookups, [Export] bindings, autoloads, _Ready ordering, spawn wiring. Assertions
/// are seed-independent so map randomness never makes them flaky.
/// </summary>
[TestSuite]
[RequireGodotRuntime]
public class MainSmokeTest
{
    private const string MainScene = "res://Scenes/Main.tscn";
    private const int GridSize = 50; // Main's GridWidth/GridHeight

    [TestCase]
    public async Task Boots_PlacesLivingPlayerWithinGrid()
    {
        using var runner = ISceneRunner.Load(MainScene, true);
        await runner.AwaitIdleFrame();

        var player = runner.Scene().GetNode<PlayerController>("Player");
        AssertObject(player).IsNotNull();

        var pos = player.GridPosition;
        AssertBool(pos.X >= 0 && pos.X < GridSize && pos.Y >= 0 && pos.Y < GridSize)
            .OverrideFailureMessage($"player spawned out of bounds at ({pos.X}, {pos.Y})")
            .IsTrue();
        AssertBool(player.IsDead).IsFalse();
    }

    [TestCase]
    public async Task Boots_SpawnsEnemiesAndStairs()
    {
        using var runner = ISceneRunner.Load(MainScene, true);
        await runner.AwaitIdleFrame();
        var main = runner.Scene();

        var enemyCount = main.GetChildren().Count(c => c is EnemyController or ArcherController);
        AssertInt(enemyCount).OverrideFailureMessage("no enemies spawned into the level").IsGreater(0);

        var hasStairs = main.GetChildren().Any(c => c is StairsController);
        AssertBool(hasStairs).OverrideFailureMessage("no stairs spawned into the level").IsTrue();
    }

    [TestCase]
    public async Task Boots_SpawnsAtMostOneFloorWeapon()
    {
        using var runner = ISceneRunner.Load(MainScene, true);
        await runner.AwaitIdleFrame();

        // Loot is now a per-floor chance (was a fixed +1/+2 pair). At most one weapon
        // drops per floor; any that drops must sit on a walkable, in-bounds tile.
        var weapons = runner.Scene().GetChildren().OfType<WeaponItem>().ToList();
        AssertInt(weapons.Count).IsLessEqual(1);
        foreach (var w in weapons)
        {
            var p = w.GridPosition;
            AssertBool(p.X is >= 0 and < GridSize && p.Y is >= 0 and < GridSize)
                .OverrideFailureMessage($"dropped weapon out of bounds at ({p.X}, {p.Y})")
                .IsTrue();
        }
    }

    // DIAGNOSTIC (archer-in-wall report): across many random layouts, no enemy may sit on a tile
    // the DungeonTileMap painted as Wall (atlas (2,0)). Reads the *rendered* tilemap — exactly what
    // the player sees — so a pass means the "stuck in wall" look is NOT a placement bug.
    [TestCase]
    public async Task EnemiesNeverRenderOnAWallTile_AcrossManyLayouts()
    {
        var wallAtlas = new Vector2I(2, 0);
        var offenders = new List<string>();

        for (var run = 0; run < 12; run++)
        {
            using var runner = ISceneRunner.Load(MainScene, true);
            await runner.AwaitIdleFrame();
            var main = runner.Scene();
            var tileMap = main.GetNode<DungeonTileMap>("DungeonTileMap");

            foreach (var enemy in main.GetChildren().OfType<ActorController>().Where(a => !a.IsPlayer))
            {
                var p = enemy.GridPosition;
                var atlas = tileMap.GetCellAtlasCoords(new Vector2I(p.X, p.Y));
                if (atlas == wallAtlas)
                    offenders.Add($"run {run}: {enemy.Name} at ({p.X},{p.Y}) on WALL");
            }
        }

        AssertBool(offenders.Count == 0)
            .OverrideFailureMessage($"enemies rendered on wall tiles:\n{string.Join("\n", offenders)}")
            .IsTrue();
    }

    [TestCase]
    public async Task Player_StepsToAWalkableNeighbour_DrivingTheTurnCycle()
    {
        using var runner = ISceneRunner.Load(MainScene, true);
        await runner.AwaitIdleFrame();
        var player = runner.Scene().GetNode<PlayerController>("Player");
        var start = player.GridPosition;

        // Drive a real player action through the live controller (also advances the turn cycle +
        // enemy AI via Main.OnTurnChanged). A spawned player always has at least one open neighbour.
        Direction[] dirs = [Direction.Right, Direction.Left, Direction.Up, Direction.Down];
        var moved = dirs.Any(player.TryMove);
        await runner.AwaitIdleFrame();

        AssertBool(moved).OverrideFailureMessage("player had no walkable neighbour to step into").IsTrue();
        AssertBool(player.GridPosition != start).IsTrue();
    }
}
