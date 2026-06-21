using RogueLike.Code.Domain.Common;

namespace RogueLike.Code.Grid.FOV;

/// <summary>
/// Interface for Fog of War calculation algorithms.
/// Allows swapping Raycasting vs Shadowcasting cleanly.
/// </summary>
public interface IFovAlgorithm
{
    /// <summary>
    /// Computes visibility from a starting position up to a certain radius,
    /// updating the provided FovMap.
    /// </summary>
    void ComputeFov(FovMap fovMap, DungeonGrid grid, GridPos origin, int radius);
}
