using System;
using System.Collections.Generic;
using Godot;

namespace RogueLike.Code.Grid.Generators;

/// <summary>
/// Generates a dungeon using Binary Space Partitioning.
/// </summary>
public static class BspDungeonGenerator
{
    private static readonly Random _rng = new Random();

    /// <summary>
    /// Executes the complete BSP algorithm on the grid map.
    /// Fills the grid with walls, carves out interconnected rooms, and returns the rooms.
    /// </summary>
    public static List<Rect2I> Generate(DungeonGrid grid, int minSplitSize = 10)
    {
        // 1. Fill completely with rock
        for (int x = 0; x < grid.Size.X; x++)
        {
            for (int y = 0; y < grid.Size.Y; y++)
            {
                grid.SetCell(new Vector2I(x, y), CellType.Wall);
            }
        }

        // 2. Partition
        var rootBounds = new Rect2I(1, 1, grid.Size.X - 2, grid.Size.Y - 2); // Leave 1 tile padded edge
        var root = new BspNode(rootBounds);
        Partition(root, minSplitSize);

        // 3. Carve & Link
        var rooms = new List<Rect2I>();
        CarveRooms(root, rooms, grid);

        return rooms;
    }

    private static void Partition(BspNode node, int minSplitSize)
    {
        // Recursion stop if space is too small to split
        if (node.Bounds.Size.X < minSplitSize * 2 || node.Bounds.Size.Y < minSplitSize * 2)
            return;

        // Try split
        bool splitHorizontal = _rng.NextDouble() > 0.5;
        
        // If width > 25% bigger than height, force vertical split
        if (node.Bounds.Size.X > node.Bounds.Size.Y * 1.25)
            splitHorizontal = false;
        else if (node.Bounds.Size.Y > node.Bounds.Size.X * 1.25)
            splitHorizontal = true;

        var max = (splitHorizontal ? node.Bounds.Size.Y : node.Bounds.Size.X) - minSplitSize;
        if (max <= minSplitSize) return;

        var splitPoint = _rng.Next(minSplitSize, max);

        if (splitHorizontal)
        {
            node.LeftChild = new BspNode(new Rect2I(node.Bounds.Position.X, node.Bounds.Position.Y, node.Bounds.Size.X, splitPoint));
            node.RightChild = new BspNode(new Rect2I(node.Bounds.Position.X, node.Bounds.Position.Y + splitPoint, node.Bounds.Size.X, node.Bounds.Size.Y - splitPoint));
        }
        else
        {
            node.LeftChild = new BspNode(new Rect2I(node.Bounds.Position.X, node.Bounds.Position.Y, splitPoint, node.Bounds.Size.Y));
            node.RightChild = new BspNode(new Rect2I(node.Bounds.Position.X + splitPoint, node.Bounds.Position.Y, node.Bounds.Size.X - splitPoint, node.Bounds.Size.Y));
        }

        Partition(node.LeftChild, minSplitSize);
        Partition(node.RightChild, minSplitSize);
    }

    private static void CarveRooms(BspNode node, List<Rect2I> roomsList, DungeonGrid grid)
    {
        if (node.IsLeaf)
        {
            // Randomly carve a room inside this space
            int w = _rng.Next(4, node.Bounds.Size.X - 2);
            int h = _rng.Next(4, node.Bounds.Size.Y - 2);
            int x = _rng.Next(node.Bounds.Position.X + 1, node.Bounds.Position.X + node.Bounds.Size.X - w - 1);
            int y = _rng.Next(node.Bounds.Position.Y + 1, node.Bounds.Position.Y + node.Bounds.Size.Y - h - 1);

            var room = new Rect2I(x, y, w, h);
            node.Room = room;
            roomsList.Add(room);

            // Carve floors
            for (int rX = room.Position.X; rX < room.End.X; rX++)
            {
                for (int rY = room.Position.Y; rY < room.End.Y; rY++)
                {
                    grid.SetCell(new Vector2I(rX, rY), CellType.Floor);
                }
            }
        }
        else
        {
            // Post-order traversal
            CarveRooms(node.LeftChild, roomsList, grid);
            CarveRooms(node.RightChild, roomsList, grid);

            // Link children
            if (node.LeftChild?.Room != null && node.RightChild?.Room != null)
            {
                ConnectRooms(grid, node.LeftChild.Room.Value, node.RightChild.Room.Value);
                // Parent inherits room connection for bubbling up
                node.Room = node.LeftChild.Room;
            }
        }
    }

    private static void ConnectRooms(DungeonGrid grid, Rect2I room1, Rect2I room2)
    {
        Vector2I center1 = GetCenter(room1);
        Vector2I center2 = GetCenter(room2);

        // Carve L-shaped corridor
        if (_rng.NextDouble() > 0.5)
        {
            CarveHorizontalTunn(grid, center1.X, center2.X, center1.Y);
            CarveVerticalTunn(grid, center1.Y, center2.Y, center2.X);
        }
        else
        {
            CarveVerticalTunn(grid, center1.Y, center2.Y, center1.X);
            CarveHorizontalTunn(grid, center1.X, center2.X, center2.Y);
        }
    }

    private static void CarveHorizontalTunn(DungeonGrid grid, int x1, int x2, int y)
    {
        for (int x = Math.Min(x1, x2); x <= Math.Max(x1, x2); x++)
        {
            grid.SetCell(new Vector2I(x, y), CellType.Floor);
        }
    }

    private static void CarveVerticalTunn(DungeonGrid grid, int y1, int y2, int x)
    {
        for (int y = Math.Min(y1, y2); y <= Math.Max(y1, y2); y++)
        {
            grid.SetCell(new Vector2I(x, y), CellType.Floor);
        }
    }

    private static Vector2I GetCenter(Rect2I room)
    {
        return new Vector2I(room.Position.X + room.Size.X / 2, room.Position.Y + room.Size.Y / 2);
    }
}
