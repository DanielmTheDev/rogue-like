using System;
using System.Collections.Generic;
using Godot;
using RogueLike.Code.Domain.Common;

namespace RogueLike.Code.Grid;

/// <summary>
/// Pure data representation of the dungeon grid.
/// No Godot Node dependency — easily testable.
/// </summary>
/// <remarks>
/// This file holds the grid's data + topology queries (walkability, corner-cut, line-of-sight).
/// All cell queries are <see cref="GridPos"/>-native; thin <c>Vector2I</c> overloads remain as a
/// transitional edge for the still-<c>Vector2I</c> callers (map generators + view renderers); they
/// retire when those migrate. The actor/movement callers moved to <c>GridPos</c> in 2.4.
/// Pixel↔grid conversion is no longer the grid's concern — it lives on the view side as
/// <c>GridConversions</c> extensions. The <c>partial</c> keyword lets the pathfinding slice
/// (<c>FindPath</c> + the hidden A* engine) live in <c>DungeonGrid.Pathfinding.cs</c>.
/// </remarks>
public partial class DungeonGrid
{
    public Vector2I Size { get; }
    public int TileSize { get; }

    private readonly CellType[,] _cells;

    public DungeonGrid(int width, int height, int tileSize = 32)
    {
        Size = new Vector2I(width, height);
        TileSize = tileSize;
        _cells = new CellType[width, height];

        FillWith(CellType.Floor);
    }

    /// <summary>
    /// Returns true if the coordinate is within the grid bounds.
    /// </summary>
    public bool IsInBounds(GridPos coord)
    {
        return coord.X >= 0
               && coord.Y >= 0
               && coord.X < Size.X
               && coord.Y < Size.Y;
    }

    /// <summary>
    /// Returns true if the coordinate is in bounds and the cell is walkable.
    /// </summary>
    public bool IsWalkable(GridPos coord)
    {
        if (!IsInBounds(coord))
            return false;

        return _cells[coord.X, coord.Y] == CellType.Floor;
    }

    /// <summary>
    /// True if an unobstructed (no walls) line runs between <paramref name="from"/> and
    /// <paramref name="to"/>. Both endpoints are excluded from the wall check. Traces the
    /// line with Bresenham's algorithm over this grid's own cells — line-of-sight is a query
    /// about the grid, so it lives with the wall data it depends on.
    /// </summary>
    public bool HasClearLine(GridPos from, GridPos to)
    {
        foreach (var cell in IntermediateCells(from, to))
        {
            if (!IsWalkable(cell))
                return false;
        }

        return true;
    }

    /// <summary>
    /// The single traversability rule: a step is allowed when the target cell is walkable and the
    /// step does not cut a wall corner. Terrain only — actor occupancy is the mover's concern.
    /// Reused by both pathfinding and movement (<c>GridMover</c>).
    /// </summary>
    public bool CanStep(GridPos from, Direction d)
        => IsWalkable(from.Step(d)) && !IsDiagonalCornerCut(from, d);

    /// <summary>
    /// Sets the cell type at the given coordinate.
    /// </summary>
    public void SetCell(GridPos coord, CellType type)
    {
        if (!IsInBounds(coord))
            return;

        _cells[coord.X, coord.Y] = type;
    }

    /// <summary>
    /// Gets the cell type at the given coordinate.
    /// Returns Wall for out-of-bounds coordinates.
    /// </summary>
    public CellType GetCell(GridPos coord)
    {
        if (!IsInBounds(coord))
            return CellType.Wall;

        return _cells[coord.X, coord.Y];
    }

    // TRANSITIONAL (DDD 2.3c): Vector2I edge for the still-Vector2I callers (map generators +
    // view renderers); retires when those migrate. Actor/movement callers moved to GridPos in 2.4.
    public bool IsInBounds(Vector2I coord) => IsInBounds(new GridPos(coord.X, coord.Y));

    public bool IsWalkable(Vector2I coord) => IsWalkable(new GridPos(coord.X, coord.Y));

    public void SetCell(Vector2I coord, CellType type) => SetCell(new GridPos(coord.X, coord.Y), type);

    public CellType GetCell(Vector2I coord) => GetCell(new GridPos(coord.X, coord.Y));

    /// <summary>
    /// True if stepping <paramref name="d"/> from <paramref name="from"/> is a diagonal that cuts a
    /// wall corner. A diagonal is only traversable when both orthogonally-adjacent cells are
    /// walkable. Cardinal directions are never corner cuts.
    /// </summary>
    private bool IsDiagonalCornerCut(GridPos from, Direction d)
    {
        if (d.Dx == 0 || d.Dy == 0)
            return false;

        return !IsWalkable(from.Step(Direction.FromDelta(d.Dx, 0)))
            || !IsWalkable(from.Step(Direction.FromDelta(0, d.Dy)));
    }

    /// <summary>
    /// Bresenham cells strictly between <paramref name="from"/> and <paramref name="to"/>
    /// (both endpoints excluded), in order. Empty when the endpoints are equal or adjacent.
    /// </summary>
    private static IEnumerable<GridPos> IntermediateCells(GridPos from, GridPos to)
    {
        var step = from.DirectionTo(to);
        var dx = Math.Abs(to.X - from.X);
        var dy = Math.Abs(to.Y - from.Y);
        var err = dx - dy;

        var current = from;
        while (current != to)
        {
            var e2 = 2 * err;
            var stepX = e2 > -dy;
            var stepY = e2 < dx;
            if (stepX) err -= dy;
            if (stepY) err += dx;
            current = new GridPos(current.X + (stepX ? step.Dx : 0), current.Y + (stepY ? step.Dy : 0));

            if (current == to)
                yield break;

            yield return current;
        }
    }

    private void FillWith(CellType type)
    {
        for (var x = 0; x < Size.X; x++)
        {
            for (var y = 0; y < Size.Y; y++)
            {
                _cells[x, y] = type;
            }
        }
    }
}
