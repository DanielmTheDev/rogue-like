using System.Collections.Generic;
using Godot;
using RogueLike.Code.Domain.Common;

namespace RogueLike.Code.View.Entities;

/// <summary>
/// View-side lookup of Godot nodes by grid position (e.g. the stairs node). Kept out of the
/// Godot-free domain <see cref="Domain.Actors.ActorRegistry"/> because
/// <see cref="Node2D"/> is a view type. Nodes free themselves; this only tracks positions.
/// </summary>
public class NodeRegistry
{
    private readonly Dictionary<GridPos, Node2D> _nodesByPosition = [];

    public void RegisterNode(Node2D node, GridPos position) => _nodesByPosition[position] = node;

    public Node2D GetNodeAt(GridPos position) => _nodesByPosition.GetValueOrDefault(position);

    public void Clear() => _nodesByPosition.Clear();
}
