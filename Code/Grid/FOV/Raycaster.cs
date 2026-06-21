using System;
using RogueLike.Code.Domain.Common;

namespace RogueLike.Code.Grid.FOV;

/// <summary>
/// Uses Bresenham's line algorithm to shoot rays in a 360-degree circle.
/// </summary>
public class Raycaster : IFovAlgorithm
{
    public void ComputeFov(FovMap fovMap, DungeonGrid grid, GridPos origin, int radius)
    {
        fovMap.ResetVisible();
        fovMap.SetVisibility(origin, VisibilityState.Visible);

        // Raycast across the perimeter of a square bound
        for (var x = -radius; x <= radius; x++)
        {
            CastRay(fovMap, grid, origin, new GridPos(origin.X + x, origin.Y - radius), radius);
            CastRay(fovMap, grid, origin, new GridPos(origin.X + x, origin.Y + radius), radius);
        }
        for (var y = -radius + 1; y < radius; y++)
        {
            CastRay(fovMap, grid, origin, new GridPos(origin.X - radius, origin.Y + y), radius);
            CastRay(fovMap, grid, origin, new GridPos(origin.X + radius, origin.Y + y), radius);
        }
    }

    private void CastRay(FovMap fovMap, DungeonGrid grid, GridPos origin, GridPos target, int radiusRadius)
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
            // Stop if outside map bounds
            if (cx < 0 || cy < 0 || cx >= grid.Size.X || cy >= grid.Size.Y)
                break;

            // Stop if we exceed the physical radius distance
            var distSq = (cx - origin.X) * (cx - origin.X) + (cy - origin.Y) * (cy - origin.Y);
            if (distSq > radiusRadius * radiusRadius)
                break;

            var pos = new GridPos(cx, cy);
            fovMap.SetVisibility(pos, VisibilityState.Visible);

            // Stop if this tile blocks vision (it's a wall)
            if (!grid.IsWalkable(pos))
                break;

            if (cx == target.X && cy == target.Y)
                break;

            var e2 = 2 * err;
            if (e2 > -dy)
            {
                err -= dy;
                cx += sx;
            }
            if (e2 < dx)
            {
                err += dx;
                cy += sy;
            }
        }
    }
}
