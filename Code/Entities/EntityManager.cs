using System.Collections.Generic;
using Godot;

namespace RogueLike.Code.Entities;

/// <summary>
/// Pure C# registry keeping track of all Actors occupying physical tiles on the grid.
/// </summary>
public class EntityManager
{
    private readonly Dictionary<Vector2I, IActor> _actorsByPosition = new();
    
    // We also keep a flat list for Turn iteration (e.g., iterating all enemies).
    private readonly List<IActor> _allActors = new();

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
    public void UpdateActorPosition(IActor actor, Vector2I oldPosition)
    {
        if (_actorsByPosition.TryGetValue(oldPosition, out var currentActor) && currentActor == actor)
        {
            _actorsByPosition.Remove(oldPosition);
        }

        _actorsByPosition[actor.GridPosition] = actor;
    }

    public bool IsOccupied(Vector2I position)
    {
        return _actorsByPosition.ContainsKey(position);
    }

    public IActor GetActorAt(Vector2I position)
    {
        return _actorsByPosition.GetValueOrDefault(position);
    }
}
