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

                int newMovementCostToNeighbor = currentNode.GCost + GetDistance(currentNode, neighbor);
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

    private List<PathNode> GetNeighbors(PathNode node, DungeonGrid grid)
    {
        var neighbors = new List<PathNode>();
        // Using 4-directional movement for now
        var directions = new[] { Vector2I.Up, Vector2I.Down, Vector2I.Left, Vector2I.Right };

        foreach (var dir in directions)
        {
            var checkPos = node.Position + dir;
            if (grid.IsWalkable(checkPos))
            {
                neighbors.Add(new PathNode(checkPos));
            }
        }
        return neighbors;
    }

    private int GetDistance(PathNode nodeA, PathNode nodeB)
    {
        // Using Manhattan distance for a 4-directional grid
        int dstX = Mathf.Abs(nodeA.Position.X - nodeB.Position.X);
        int dstY = Mathf.Abs(nodeA.Position.Y - nodeB.Position.Y);
        return dstX + dstY;
    }
}
