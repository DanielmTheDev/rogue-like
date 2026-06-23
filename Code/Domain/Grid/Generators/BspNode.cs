using RogueLike.Domain.Common;

namespace RogueLike.Domain.Grid.Generators;

/// <summary>
/// A node representing a partitioned space in the BSP dungeon tree.
/// </summary>
public class BspNode
{
    public GridRect Bounds { get; }
    public BspNode LeftChild { get; set; }
    public BspNode RightChild { get; set; }

    // The physical floor carved out inside these bounds
    public GridRect? Room { get; set; }

    public BspNode(GridRect bounds)
    {
        Bounds = bounds;
    }

    public bool IsLeaf => LeftChild == null && RightChild == null;
}
