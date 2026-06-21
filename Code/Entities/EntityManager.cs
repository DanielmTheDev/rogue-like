using System.Collections.Generic;
using Godot;
using RogueLike.Code.Domain.Common;

namespace RogueLike.Code.Entities;

/// <summary>
/// Pure C# registry keeping track of all Actors occupying physical tiles on the grid.
/// </summary>
// Actor positions are the domain <see cref="GridPos"/>. The node map still stores Godot
// <c>Node2D</c>s (view concern) keyed by the same <c>GridPos</c>; extracting that node registry
// to the view side is a separate cleanup.
public class EntityManager
{
    private readonly Dictionary<GridPos, IActor> _actorsByPosition = [];
    private readonly Dictionary<GridPos, Node2D> _nodesByPosition = [];

    // We also keep a flat list for Turn iteration (e.g., iterating all enemies).
    private readonly List<IActor> _allActors = [];

    public IReadOnlyList<IActor> AllActors => _allActors;

    public void RegisterActor(IActor actor)
    {
        _allActors.Add(actor);
        _actorsByPosition[actor.GridPosition] = actor;
    }

    public void UnregisterActor(IActor actor)
    {
        _allActors.Remove(actor);
        if (_actorsByPosition.TryGetValue(actor.GridPosition, out var currentActor) && currentActor == actor)
        {
            _actorsByPosition.Remove(actor.GridPosition);
        }
    }

    /// <summary>
    /// Call this whenever an actor successfully moves.
    /// </summary>
    public void UpdateActorPosition(IActor actor, GridPos oldPosition, GridPos newPosition)
    {
        if (_actorsByPosition.TryGetValue(oldPosition, out var currentActor) && currentActor == actor)
        {
            _actorsByPosition.Remove(oldPosition);
        }

        _actorsByPosition[newPosition] = actor;
    }

    public void RegisterNode(Node2D node, GridPos position)
    {
        _nodesByPosition[position] = node;
    }

    public bool IsOccupied(GridPos position)
    {
        return _actorsByPosition.ContainsKey(position);
    }

    public Node2D GetNodeAt(GridPos position)
    {
        return _nodesByPosition.GetValueOrDefault(position);
    }

    public IActor GetActorAt(GridPos position)
    {
        return _actorsByPosition.GetValueOrDefault(position);
    }

    public void ClearAll()
    {
        // We only clear the lookup dictionaries. The nodes themselves will be QueueFree'd from Main.
        _allActors.Clear();
        _actorsByPosition.Clear();
        _nodesByPosition.Clear();
    }
}
