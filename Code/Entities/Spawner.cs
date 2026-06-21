using System.Collections.Generic;
using Godot;
using RogueLike.Code.Enemies;
using RogueLike.Code.Grid;
using RogueLike.Code.Grid.FOV;
using RogueLike.Code.Player;
using RogueLike.Code.Items;
using RogueLike.Code.Resources;
using RogueLike.Code.View;
using RogueLike.Code.World;

namespace RogueLike.Code.Entities;

/// <summary>
/// Static utility handling the safe instantiation and placement of entities on the grid.
/// </summary>
public static class Spawner
{
    private static readonly System.Random _rng = new();

    public static void PlacePlayerOnLevel(
        PlayerController player,
        Rect2I startRoom,
        DungeonGrid grid,
        EntityManager entityManager,
        FovMap fovMap)
    {
        var centerPos = new Vector2I(startRoom.Position.X + startRoom.Size.X / 2, startRoom.Position.Y + startRoom.Size.Y / 2);
        player.PlaceOnLevel(grid, entityManager, fovMap, centerPos.ToGridPos());
    }

    public static void SpawnEnemies(
        Node parentNode,
        PackedScene goblinScene,
        PackedScene archerScene,
        List<Rect2I> rooms,
        DungeonGrid grid,
        EntityManager entityManager,
        int dungeonLevel,
        LevelSettings levelSettings)
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

            var spawnedPositions = new HashSet<Vector2I>();

            for (var j = 0; j < numberOfEnemies; j++)
            {
                Vector2I spawnPos;
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
                    SpawnGoblin(parentNode, goblinScene, grid, entityManager, spawnPos, i * 10 + j);
                else
                    SpawnArcher(parentNode, archerScene, grid, entityManager, spawnPos, i * 10 + j);
            }
        }
    }

    public static void SpawnGoblin(
        Node parent, PackedScene scene, DungeonGrid grid,
        EntityManager entityManager, Vector2I pos, int index)
    {
        var enemy = scene.Instantiate<EnemyController>();
        enemy.Name = $"Goblin_{index}";
        parent.AddChild(enemy);
        enemy.Initialize(grid, entityManager, pos.ToGridPos());
    }

    public static void SpawnArcher(
        Node parent, PackedScene scene, DungeonGrid grid,
        EntityManager entityManager, Vector2I pos, int index)
    {
        var archer = scene.Instantiate<ArcherController>();
        archer.Name = $"Archer_{index}";
        parent.AddChild(archer);
        archer.Initialize(grid, entityManager, pos.ToGridPos());
    }

    public static void SpawnHealingPotion(
        Node parent, PackedScene potionScene, DungeonGrid grid,
        ItemManager itemManager, Vector2I pos)
    {
        var potion = potionScene.Instantiate<Items.Consumables.HealingPotion>();
        parent.AddChild(potion);
        potion.Initialize(itemManager, pos);
    }

    public static void SpawnPotions(
        Node parentNode,
        PackedScene potionScene,
        List<Rect2I> rooms,
        DungeonGrid grid,
        ItemManager itemManager)
    {
        // Spawn 1-2 potions per dungeon in random rooms (skip first room where player starts)
        var potionCount = _rng.Next(1, 3);
        for (var i = 0; i < potionCount; i++)
        {
            var room = rooms[_rng.Next(1, rooms.Count)];
            var spawnPos = RandomFloorTile(room);
            SpawnHealingPotion(parentNode, potionScene, grid, itemManager, spawnPos);
        }
    }

    public static Vector2I RandomFloorTile(Rect2I room)
    {
        var rx = _rng.Next(room.Position.X, room.Position.X + room.Size.X);
        var ry = _rng.Next(room.Position.Y, room.Position.Y + room.Size.Y);
        return new Vector2I(rx, ry);
    }

    public static void SpawnStairs(Node parent, PackedScene scene, Rect2I room, EntityManager entityManager, DungeonGrid grid)
    {
        var stairs = scene.Instantiate<StairsController>();
        var position = new Vector2I(room.Position.X + room.Size.X / 2, room.Position.Y + room.Size.Y / 2);

        stairs.Initialize(position);
        stairs.Position = position.ToGridPos().ToWorldCenter(grid.TileSize);

        parent.AddChild(stairs);
        entityManager.RegisterNode(stairs, position.ToGridPos());
    }
}
