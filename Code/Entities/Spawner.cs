using System.Collections.Generic;
using Godot;
using RogueLike.Code.Enemies;
using RogueLike.Code.Grid;
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
        ItemManager itemManager)
    {
        var centerPos = new Vector2I(startRoom.Position.X + startRoom.Size.X / 2, startRoom.Position.Y + startRoom.Size.Y / 2);
        player.Initialize(grid, entityManager, turnManager, itemManager, centerPos);
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
            var spawnPos = RandomFloorTile(room);

            // Alternate enemy types: odd rooms get goblins, even rooms get archers
            if (i % 2 == 1)
                SpawnGoblin(parentNode, goblinScene, grid, entityManager, spawnPos, i);
            else
                SpawnArcher(parentNode, archerScene, grid, entityManager, spawnPos, i);
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
