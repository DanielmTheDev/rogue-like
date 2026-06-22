using System;
using System.Collections.Generic;
using RogueLike.Code.Domain.Common;

namespace RogueLike.Code.Domain.Grid.Generators;

/// <summary>
/// Generates a dungeon using Binary Space Partitioning.
/// </summary>
public static class BspDungeonGenerator
{
    private static readonly Random _rng = new();

    /// <summary>
    /// Executes the complete BSP algorithm on the grid map.
    /// Fills the grid with walls, carves out interconnected rooms, and returns the rooms.
    /// </summary>
    public static List<GridRect> Generate(DungeonGrid grid, int minSplitSize = 10)
    {
        // 1. Fill completely with rock
        for (var x = 0; x < grid.Width; x++)
        {
            for (var y = 0; y < grid.Height; y++)
            {
                grid.SetCell(new GridPos(x, y), CellType.Wall);
            }
        }

        // 2. Partition
        var rootBounds = new GridRect(1, 1, grid.Width - 2, grid.Height - 2); // Leave 1 tile padded edge
        var root = new BspNode(rootBounds);
        Partition(root, minSplitSize);

        // 3. Carve & Link
        var rooms = new List<GridRect>();
        CarveRooms(root, rooms, grid);

        return rooms;
    }

    private static void Partition(BspNode node, int minSplitSize)
    {
        // Recursion stop if space is too small to split
        if (node.Bounds.Width < minSplitSize * 2 || node.Bounds.Height < minSplitSize * 2)
            return;

        // Try split
        var splitHorizontal = _rng.NextDouble() > 0.5;

        // If width > 25% bigger than height, force vertical split
        if (node.Bounds.Width > node.Bounds.Height * 1.25)
            splitHorizontal = false;
        else if (node.Bounds.Height > node.Bounds.Width * 1.25)
            splitHorizontal = true;

        var max = (splitHorizontal ? node.Bounds.Height : node.Bounds.Width) - minSplitSize;
        if (max <= minSplitSize) return;

        var splitPoint = _rng.Next(minSplitSize, max);

        if (splitHorizontal)
        {
            node.LeftChild = new BspNode(new GridRect(node.Bounds.X, node.Bounds.Y, node.Bounds.Width, splitPoint));
            node.RightChild = new BspNode(new GridRect(node.Bounds.X, node.Bounds.Y + splitPoint, node.Bounds.Width, node.Bounds.Height - splitPoint));
        }
        else
        {
            node.LeftChild = new BspNode(new GridRect(node.Bounds.X, node.Bounds.Y, splitPoint, node.Bounds.Height));
            node.RightChild = new BspNode(new GridRect(node.Bounds.X + splitPoint, node.Bounds.Y, node.Bounds.Width - splitPoint, node.Bounds.Height));
        }

        Partition(node.LeftChild, minSplitSize);
        Partition(node.RightChild, minSplitSize);
    }

    private static void CarveRooms(BspNode node, List<GridRect> roomsList, DungeonGrid grid)
    {
        if (node.IsLeaf)
        {
            // Randomly carve a room inside this space
            var w = _rng.Next(4, node.Bounds.Width - 2);
            var h = _rng.Next(4, node.Bounds.Height - 2);
            var x = _rng.Next(node.Bounds.X + 1, node.Bounds.X + node.Bounds.Width - w - 1);
            var y = _rng.Next(node.Bounds.Y + 1, node.Bounds.Y + node.Bounds.Height - h - 1);

            var room = new GridRect(x, y, w, h);
            node.Room = room;
            roomsList.Add(room);

            // Carve floors
            for (var rX = room.X; rX < room.End.X; rX++)
            {
                for (var rY = room.Y; rY < room.End.Y; rY++)
                {
                    grid.SetCell(new GridPos(rX, rY), CellType.Floor);
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

    private static void ConnectRooms(DungeonGrid grid, GridRect room1, GridRect room2)
    {
        var center1 = room1.Center;
        var center2 = room2.Center;

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
        for (var x = Math.Min(x1, x2); x <= Math.Max(x1, x2); x++)
        {
            grid.SetCell(new GridPos(x, y), CellType.Floor);
        }
    }

    private static void CarveVerticalTunn(DungeonGrid grid, int y1, int y2, int x)
    {
        for (var y = Math.Min(y1, y2); y <= Math.Max(y1, y2); y++)
        {
            grid.SetCell(new GridPos(x, y), CellType.Floor);
        }
    }
}
