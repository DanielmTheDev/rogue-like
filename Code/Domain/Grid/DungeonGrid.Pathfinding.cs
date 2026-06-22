using System;
using System.Collections.Generic;
using System.Linq;
using RogueLike.Code.Domain.Common;

namespace RogueLike.Code.Domain.Grid;

/// <summary>
/// Pathfinding slice of <see cref="DungeonGrid"/> — navigation is a query over the grid's own
/// cells, so it lives with the wall data. Public entry is <see cref="FindPath(GridPos, GridPos)"/>;
/// the A* itself runs in the hidden nested <c>PathSearch</c>.
/// </summary>
public partial class DungeonGrid
{
    /// <summary>
    /// Shortest 8-directional path between two cells (A* over the grid's own walls), or null if
    /// none exists. The start cell is excluded; an empty list means start == end. The search runs
    /// in a hidden per-search object so its mutable state never lives on the grid.
    /// </summary>
    public List<GridPos> FindPath(GridPos from, GridPos to) => new PathSearch(this).Run(from, to);

    /// <summary>The walkable 8-neighbours of <paramref name="p"/> (corner-cuts excluded).</summary>
    private IEnumerable<GridPos> WalkableNeighbors(GridPos p)
        => Direction.AllEight.Where(d => CanStep(p, d)).Select(p.Step);

    /// <summary>
    /// A* over the grid's cells. One short-lived instance per search owns the mutable open/closed
    /// state, so it never lives as fields on the long-lived grid. Hidden: it is a private nested
    /// type, so only <see cref="DungeonGrid"/> can construct it — the only entry point is
    /// <see cref="FindPath(GridPos, GridPos)"/>.
    /// </summary>
    private sealed class PathSearch(DungeonGrid grid)
    {
        public List<GridPos> Run(GridPos start, GridPos end)
        {
            var open = new List<PathNode> { new(start) };
            var closed = new HashSet<GridPos>();

            while (open.Count > 0)
            {
                var current = LowestCost(open);
                open.Remove(current);
                closed.Add(current.Position);

                if (current.Position == end)
                    return Retrace(current);

                ExpandFrom(current, end, open, closed);
            }

            return null; // No path found.
        }

        // Each walkable neighbour enters the open list at most once (on first discovery); already-open
        // or closed cells are left untouched. Mirrors the original Pathfinder behaviour exactly.
        private void ExpandFrom(PathNode current, GridPos end, List<PathNode> open, HashSet<GridPos> closed)
        {
            foreach (var pos in grid.WalkableNeighbors(current.Position))
            {
                if (closed.Contains(pos) || open.Any(n => n.Position == pos))
                    continue;

                open.Add(new PathNode(pos)
                {
                    GCost = current.GCost + Distance(current.Position, pos),
                    HCost = Distance(pos, end),
                    Parent = current,
                });
            }
        }

        private static PathNode LowestCost(List<PathNode> open)
            => open.OrderBy(n => n.FCost).ThenBy(n => n.HCost).First();

        private static List<GridPos> Retrace(PathNode end)
        {
            var path = new List<GridPos>();
            for (var node = end; node.Parent != null; node = node.Parent)
                path.Add(node.Position);
            path.Reverse();
            return path;
        }

        // Octile distance for 8-directional movement: D=10 cardinal, D2=14 diagonal.
        // Keeps the A* heuristic admissible and gives diagonal steps the correct cost.
        private static int Distance(GridPos a, GridPos b)
        {
            const int D = 10;
            const int D2 = 14;
            var dx = Math.Abs(a.X - b.X);
            var dy = Math.Abs(a.Y - b.Y);
            var min = Math.Min(dx, dy);
            return D2 * min + D * (Math.Max(dx, dy) - min);
        }

        private sealed class PathNode(GridPos position)
        {
            public GridPos Position { get; } = position;
            public PathNode Parent { get; init; }
            public int GCost { get; init; }
            public int HCost { get; init; }
            public int FCost => GCost + HCost;
        }
    }
}
