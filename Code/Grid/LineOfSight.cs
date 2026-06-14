using System;
using Godot;

namespace RogueLike.Code.Grid;

/// <summary>
/// Pure C# utility for checking unobstructed line-of-sight between two grid tiles.
/// Uses Bresenham's line algorithm — the same core logic as the Raycaster,
/// but answering a simple boolean question instead of mutating a FovMap.
/// </summary>
public static class LineOfSight
{
    /// <summary>
    /// Returns true if there is a clear, walkable path between origin and target
    /// with no walls blocking the line. Both endpoints are excluded from the wall check.
    /// </summary>
    public static bool HasClearLine(DungeonGrid grid, Vector2I origin, Vector2I target)
    {
        var dx = Math.Abs(target.X - origin.X);
        var dy = Math.Abs(target.Y - origin.Y);
        var sx = origin.X < target.X ? 1 : -1;
        var sy = origin.Y < target.Y ? 1 : -1;
        var err = dx - dy;

        var cx = origin.X;
        var cy = origin.Y;

        while (true)
        {
            if (cx == target.X && cy == target.Y)
                return true; // Reached target without hitting a wall

            var e2 = 2 * err;
            if (e2 > -dy) { err -= dy; cx += sx; }
            if (e2 < dx)  { err += dx; cy += sy; }

            var pos = new Vector2I(cx, cy);

            // If we've arrived at the target, that's fine — don't check walkability
            if (cx == target.X && cy == target.Y)
                return true;

            // If any intermediate tile is a wall, LOS is blocked
            if (!grid.IsWalkable(pos))
                return false;
        }
    }

    /// <summary>
    /// Manhattan distance between two grid positions.
    /// </summary>
    public static int ManhattanDistance(Vector2I a, Vector2I b)
    {
        return Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);
    }
}
