using System.Collections.Generic;
using Godot;

namespace RogueLike.Code.Entities;

/// <summary>
/// Pure C# registry keeping track of all Actors occupying physical tiles on the grid.
/// </summary>
public class EntityManager
{
    private readonly Dictionary<Vector2I, IActor> _actorsByPosition = new();
    private readonly Dictionary<Vector2I, Node2D> _nodesByPosition = new();
    
    // We also keep a flat list for Turn iteration (e.g., iterating all enemies).
    private readonly List<IActor> _allActors = new();

    public IReadOnlyList<IActor> AllActors => _allActors;

    public void RegisterActor(IActor actor)
    {
        _allActors.Add(actor);
        _actorsByPosition[actor.GridPosition] = actor;
        
        if (actor is Node2D node)
        {
            _nodesByPosition[actor.GridPosition] = node;
        }
    }

    public void UnregisterActor(IActor actor)
    {
        _allActors.Remove(actor);
        if (_actorsByPosition.TryGetValue(actor.GridPosition, out var currentActor) && currentActor == actor)
        {
            _actorsByPosition.Remove(actor.GridPosition);
        }
        if (_nodesByPosition.ContainsKey(actor.GridPosition))
        {
            _nodesByPosition.Remove(actor.GridPosition);
        }
    }

    /// <summary>
    /// Call this whenever an actor successfully moves.
    /// </summary>
    public void UpdateActorPosition(IActor actor, Vector2I oldPosition, Vector2I newPosition)
    {
        if (_actorsByPosition.TryGetValue(oldPosition, out var currentActor) && currentActor == actor)
        {
            _actorsByPosition.Remove(oldPosition);
            _nodesByPosition.Remove(oldPosition);
        }

        _actorsByPosition[newPosition] = actor;
        if (actor is Node2D node)
        {
            _nodesByPosition[newPosition] = node;
        }
    }
    
    public void RegisterNode(Node2D node, Vector2I position)
    {
        _nodesByPosition[position] = node;
    }

    public bool IsOccupied(Vector2I position)
    {
        return _actorsByPosition.ContainsKey(position);
    }

    public Node2D GetNodeAt(Vector2I position)
    {
        return _nodesByPosition.GetValueOrDefault(position);
    }

    public IActor GetActorAt(Vector2I position)
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
