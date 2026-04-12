using System.Collections.Generic;
using Godot;
using RogueLike.Code.Enemies;
using RogueLike.Code.Grid;
using RogueLike.Code.Player;
using RogueLike.Code.TurnContext;

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
        TurnManager turnManager)
    {
        var centerPos = new Vector2I(startRoom.Position.X + startRoom.Size.X / 2, startRoom.Position.Y + startRoom.Size.Y / 2);
        player.Initialize(grid, entityManager, turnManager, centerPos);
        
        // Attach Camera2D dynamically
        var camera = new Camera2D
        {
            Zoom = new Vector2(1.5f, 1.5f), // 150% zoom is standard rogue
            PositionSmoothingEnabled = true
        };
        player.AddChild(camera);
    }

    public static void SpawnEnemies(
        Node parentNode, 
        PackedScene enemyScene, 
        List<Rect2I> rooms, 
        DungeonGrid grid, 
        EntityManager entityManager)
    {
        // Spawn one enemy per generated room (skipping the player's room 0)
        for (int i = 1; i < rooms.Count; i++)
        {
            var room = rooms[i];
            
            // Pick a random tile anywhere inside the room floor
            int rx = _rng.Next(room.Position.X, room.Position.X + room.Size.X);
            int ry = _rng.Next(room.Position.Y, room.Position.Y + room.Size.Y);
            
            var enemy = enemyScene.Instantiate<EnemyController>();
            enemy.Name = $"Enemy_{i}";
            parentNode.AddChild(enemy);

            enemy.Initialize(grid, entityManager, new Vector2I(rx, ry));
        }
    }
}
