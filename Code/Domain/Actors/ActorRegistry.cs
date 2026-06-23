using System.Collections.Generic;
using RogueLike.Domain.Common;

namespace RogueLike.Domain.Actors;

/// <summary>
/// Pure C# (Godot-free) registry of all actors occupying tiles on the grid, keyed by
/// <see cref="GridPos"/>. Guards single-occupancy and exposes a flat list for turn iteration.
/// The Godot <c>Node2D</c> lookup that used to live alongside this is now the view-side
/// <c>NodeRegistry</c>.
/// </summary>
public class ActorRegistry
{
    private readonly Dictionary<GridPos, IActor> _actorsByPosition = [];

    // A flat list for turn iteration (e.g. iterating all enemies).
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

    public bool IsOccupied(GridPos position)
    {
        return _actorsByPosition.ContainsKey(position);
    }

    public IActor GetActorAt(GridPos position)
    {
        return _actorsByPosition.GetValueOrDefault(position);
    }

    public void ClearAll()
    {
        _allActors.Clear();
        _actorsByPosition.Clear();
    }
}
