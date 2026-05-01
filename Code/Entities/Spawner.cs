using System.Collections.Generic;
using Godot;
using RogueLike.Code.Enemies;
using RogueLike.Code.Grid;
using RogueLike.Code.Grid.FOV;
using RogueLike.Code.Player;
using RogueLike.Code.TurnContext;
using RogueLike.Code.Items;

namespace RogueLike.Code.Entities;

/// <summary>
/// Static utility handling the safe instantiation and placement of entities on the grid.
/// </summary>
public static class Spawner
{
    private static readonly System.Random _rng = new System.Random();

    public static void InitializePlayer(
        PlayerController player,
        Rect2I startRoom,
        DungeonGrid grid,
        EntityManager entityManager,
        TurnManager turnManager,
        ItemManager itemManager,
        FovMap fovMap)
    {
        var centerPos = new Vector2I(startRoom.Position.X + startRoom.Size.X / 2, startRoom.Position.Y + startRoom.Size.Y / 2);
        player.Initialize(grid, entityManager, turnManager, itemManager, fovMap, centerPos);
    }

    public static void SpawnEnemies(
        Node parentNode,
        PackedScene goblinScene,
        PackedScene archerScene,
        List<Rect2I> rooms,
        DungeonGrid grid,
        EntityManager entityManager)
    {
        for (int i = 1; i < rooms.Count; i++)
        {
            var room = rooms[i];
            int numberOfEnemies = _rng.Next(2, 4); // Spawn 2 or 3 enemies
            var spawnedPositions = new HashSet<Vector2I>();

            for (int j = 0; j < numberOfEnemies; j++)
            {
                Vector2I spawnPos;
                int attempts = 0;
                // Avoid spawning multiple enemies on the same tile or in walls
                do
                {
                    spawnPos = RandomFloorTile(room);
                    attempts++;
                } while (spawnedPositions.Contains(spawnPos) && attempts < 100);

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
        enemy.Initialize(grid, entityManager, pos);
    }

    public static void SpawnArcher(
        Node parent, PackedScene scene, DungeonGrid grid,
        EntityManager entityManager, Vector2I pos, int index)
    {
        var archer = scene.Instantiate<ArcherController>();
        archer.Name = $"Archer_{index}";
        parent.AddChild(archer);
        archer.Initialize(grid, entityManager, pos);
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
        int potionCount = _rng.Next(1, 3);
        for (int i = 0; i < potionCount; i++)
        {
            var room = rooms[_rng.Next(1, rooms.Count)];
            var spawnPos = RandomFloorTile(room);
            SpawnHealingPotion(parentNode, potionScene, grid, itemManager, spawnPos);
        }
    }

    public static Vector2I RandomFloorTile(Rect2I room)
    {
        int rx = _rng.Next(room.Position.X, room.Position.X + room.Size.X);
        int ry = _rng.Next(room.Position.Y, room.Position.Y + room.Size.Y);
        return new Vector2I(rx, ry);
    }
}
