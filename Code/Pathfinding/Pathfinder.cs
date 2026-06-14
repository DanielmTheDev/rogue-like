using System.Collections.Generic;
using System.Linq;
using Godot;
using RogueLike.Code.Grid;

namespace RogueLike.Code.Pathfinding;

public class Pathfinder
{
    /// <summary>
    /// A node for the A* algorithm, representing a tile on the grid.
    /// </summary>
    private class PathNode
    {
        public Vector2I Position { get; }
        public PathNode Parent { get; set; }
        public int GCost { get; set; } // Distance from starting node
        public int HCost { get; set; } // Heuristic distance to end node
        public int FCost => GCost + HCost; // Total cost

        public PathNode(Vector2I position)
        {
            Position = position;
        }
    }

    /// <summary>
    /// Finds the shortest path between two points on the grid.
    /// </summary>
    /// <returns>A list of grid coordinates representing the path, or null if no path is found.</returns>
    public List<Vector2I> FindPath(Vector2I start, Vector2I end, DungeonGrid grid)
    {
        var startNode = new PathNode(start);
        var endNode = new PathNode(end);

        var openList = new List<PathNode> { startNode };
        var closedList = new HashSet<Vector2I>();

        while (openList.Count > 0)
        {
            var currentNode = openList.OrderBy(n => n.FCost).ThenBy(n => n.HCost).First();

            openList.Remove(currentNode);
            closedList.Add(currentNode.Position);

            if (currentNode.Position == endNode.Position)
            {
                return RetracePath(startNode, currentNode);
            }

            var neighbors = GetNeighbors(currentNode, grid);
            foreach (var neighbor in neighbors)
            {
                if (closedList.Contains(neighbor.Position))
                {
                    continue;
                }

                var newMovementCostToNeighbor = currentNode.GCost + GetDistance(currentNode, neighbor);
                if (newMovementCostToNeighbor < neighbor.GCost || !openList.Any(n => n.Position == neighbor.Position))
                {
                    neighbor.GCost = newMovementCostToNeighbor;
                    neighbor.HCost = GetDistance(neighbor, endNode);
                    neighbor.Parent = currentNode;

                    if (!openList.Any(n => n.Position == neighbor.Position))
                    {
                        openList.Add(neighbor);
                    }
                }
            }
        }

        return null; // No path found
    }

    private List<Vector2I> RetracePath(PathNode startNode, PathNode endNode)
    {
        var path = new List<Vector2I>();
        var currentNode = endNode;

        while (currentNode != startNode)
        {
            path.Add(currentNode.Position);
            currentNode = currentNode.Parent;
        }
        path.Reverse();
        return path;
    }

    private static readonly Vector2I[] Directions =
    [
        Vector2I.Up, Vector2I.Down, Vector2I.Left, Vector2I.Right,
        new(-1, -1), new(1, -1), new(-1, 1), new(1, 1)
    ];

    private List<PathNode> GetNeighbors(PathNode node, DungeonGrid grid)
    {
        var neighbors = new List<PathNode>();

        foreach (var dir in Directions)
        {
            var checkPos = node.Position + dir;
            if (!grid.IsWalkable(checkPos))
                continue;
            // Disallow diagonals that cut a wall corner (matches GridMover).
            if (grid.IsDiagonalCornerCut(node.Position, dir))
                continue;

            neighbors.Add(new PathNode(checkPos));
        }
        return neighbors;
    }

    private int GetDistance(PathNode nodeA, PathNode nodeB)
    {
        // Octile distance for 8-directional movement: D=10 cardinal, D2=14 diagonal.
        // Keeps the A* heuristic admissible and gives diagonal steps the correct cost.
        const int D = 10;
        const int D2 = 14;
        var dstX = Mathf.Abs(nodeA.Position.X - nodeB.Position.X);
        var dstY = Mathf.Abs(nodeA.Position.Y - nodeB.Position.Y);
        var min = Mathf.Min(dstX, dstY);
        return D2 * min + D * (Mathf.Max(dstX, dstY) - min);
    }
}
