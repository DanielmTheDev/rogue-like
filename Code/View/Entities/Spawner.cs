using System.Collections.Generic;
using Godot;
using RogueLike.Domain.Common;
using RogueLike.Code.View.Enemies;
using RogueLike.Domain.Grid;
using RogueLike.Domain.Grid.FOV;
using RogueLike.Domain.Actors;
using RogueLike.Code.View.Player;
using RogueLike.Domain.Equipment;
using RogueLike.Domain.Items;
using RogueLike.Domain.Loot;
using RogueLike.Code.View.Items;
using RogueLike.Code.View.Resources;
using RogueLike.Code.View.World;

namespace RogueLike.Code.View.Entities;

/// <summary>
/// Static utility handling the safe instantiation and placement of entities on the grid.
/// </summary>
public static class Spawner
{
    private static readonly System.Random _rng = new();

    public static void PlacePlayerOnLevel(
        PlayerController player,
        GridRect startRoom,
        DungeonGrid grid,
        ActorRegistry actorRegistry,
        FovMap fovMap,
        NodeRegistry nodeRegistry)
    {
        player.PlaceOnLevel(grid, actorRegistry, fovMap, nodeRegistry, startRoom.Center);
    }

    public static void SpawnEnemies(
        Node parentNode,
        PackedScene goblinScene,
        PackedScene archerScene,
        List<GridRect> rooms,
        DungeonGrid grid,
        ActorRegistry actorRegistry,
        int dungeonLevel,
        LevelSettings levelSettings,
        FovMap fovMap)
    {
        for (var i = 1; i < rooms.Count; i++)
        {
            var room = rooms[i];

            // Use the LevelSettings resource to determine enemy count
            var baseCount = levelSettings.BaseEnemyCountModifier;
            var min = levelSettings.MinEnemiesPerRoom;
            var max = levelSettings.MaxEnemiesPerRoom;
            var scaling = levelSettings.DifficultyScaling;

            var numberOfEnemies = _rng.Next(min, max + 1) + baseCount + (int)((dungeonLevel - 1) * scaling);

            var spawnedPositions = new HashSet<GridPos>();

            for (var j = 0; j < numberOfEnemies; j++)
            {
                GridPos spawnPos;
                var attempts = 0;
                // Avoid spawning multiple enemies on the same tile or in walls
                do
                {
                    spawnPos = RandomFloorTile(room);
                    attempts++;
                } while ((spawnedPositions.Contains(spawnPos) || !grid.IsWalkable(spawnPos)) && attempts < 100);

                if (attempts >= 100) continue; // Failsafe for very small rooms

                spawnedPositions.Add(spawnPos);

                // Alternate enemy types
                if (j % 2 == 0)
                    SpawnGoblin(parentNode, goblinScene, grid, actorRegistry, spawnPos, i * 10 + j, fovMap);
                else
                    SpawnArcher(parentNode, archerScene, grid, actorRegistry, spawnPos, i * 10 + j, fovMap);
            }
        }
    }

    public static void SpawnGoblin(
        Node parent, PackedScene scene, DungeonGrid grid,
        ActorRegistry actorRegistry, GridPos pos, int index, FovMap fovMap)
    {
        var enemy = scene.Instantiate<EnemyController>();
        enemy.Name = $"Goblin_{index}";
        parent.AddChild(enemy);
        enemy.Initialize(grid, actorRegistry, pos, fovMap);
    }

    public static void SpawnArcher(
        Node parent, PackedScene scene, DungeonGrid grid,
        ActorRegistry actorRegistry, GridPos pos, int index, FovMap fovMap)
    {
        var archer = scene.Instantiate<ArcherController>();
        archer.Name = $"Archer_{index}";
        parent.AddChild(archer);
        archer.Initialize(grid, actorRegistry, pos, fovMap);
    }

    public static void SpawnHealingPotion(
        Node parent, PackedScene potionScene, DungeonGrid grid,
        FloorItems floorItems, GridPos pos)
    {
        var potion = potionScene.Instantiate<Items.Consumables.HealingPotion>();
        parent.AddChild(potion);
        potion.Initialize(floorItems, pos);
    }

    public static void SpawnPotions(
        Node parentNode,
        PackedScene potionScene,
        List<GridRect> rooms,
        DungeonGrid grid,
        FloorItems floorItems)
    {
        // Spawn 1-2 potions per dungeon in random rooms (skip first room where player starts)
        var potionCount = _rng.Next(1, 3);
        for (var i = 0; i < potionCount; i++)
        {
            var room = rooms[_rng.Next(1, rooms.Count)];
            var spawnPos = RandomFloorTile(room);
            SpawnHealingPotion(parentNode, potionScene, grid, floorItems, spawnPos);
        }
    }

    public static void SpawnWeapon(
        Node parent, PackedScene weaponScene, FloorItems floorItems,
        GridPos pos, Weapon weapon)
    {
        var item = weaponScene.Instantiate<WeaponItem>();
        item.Configure(weapon);
        parent.AddChild(item);
        item.Initialize(floorItems, pos);
    }

    public static void SpawnFloorLoot(
        Node parentNode, PackedScene weaponScene, List<GridRect> rooms,
        DungeonGrid grid, FloorItems floorItems,
        WeaponLootTable lootTable, int dungeonLevel, IRng rng)
    {
        if (lootTable.Roll(dungeonLevel) is not { } weapon)
            return;

        // Place in a random non-start room (skip rooms[0]) on a walkable tile, using the
        // injected rng so placement is reproducible with the loot seed.
        var room = rooms[rng.Next(1, rooms.Count)];
        GridPos pos;
        var attempts = 0;
        do
        {
            pos = new GridPos(rng.Next(room.X, room.X + room.Width), rng.Next(room.Y, room.Y + room.Height));
            attempts++;
        } while (!grid.IsWalkable(pos) && attempts < 100);

        if (attempts >= 100) return; // Failsafe for irregular rooms
        SpawnWeapon(parentNode, weaponScene, floorItems, pos, weapon);
    }

    public static GridPos RandomFloorTile(GridRect room)
    {
        var rx = _rng.Next(room.X, room.X + room.Width);
        var ry = _rng.Next(room.Y, room.Y + room.Height);
        return new GridPos(rx, ry);
    }

    public static void SpawnStairs(Node parent, PackedScene scene, GridRect room, NodeRegistry nodeRegistry, DungeonGrid grid)
    {
        var stairs = scene.Instantiate<StairsController>();
        var center = room.Center;

        stairs.Initialize(center.ToVector2I());
        stairs.Position = center.ToWorldCenter(grid.TileSize);

        parent.AddChild(stairs);
        nodeRegistry.RegisterNode(stairs, center);
    }
}
