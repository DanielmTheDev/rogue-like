using RogueLike.Domain.Common;

namespace RogueLike.Domain.Actors;

/// <summary>
/// Interface for any dynamic entity residing on the grid (Player, Enemy).
/// </summary>
public interface IActor
{
    GridPos GridPosition { get; }

    /// <summary>
    /// Identifies if this actor is the primary player.
    /// </summary>
    bool IsPlayer { get; }
}
