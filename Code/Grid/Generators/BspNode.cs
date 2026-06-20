using Godot;

namespace RogueLike.Code.Grid.Generators;

/// <summary>
/// A node representing a partitioned space in the BSP dungeon tree.
/// </summary>
public class BspNode
{
    public Rect2I Bounds { get; }
    public BspNode LeftChild { get; set; }
    public BspNode RightChild { get; set; }

    // The physical floor carved out inside these bounds
    public Rect2I? Room { get; set; }

    public BspNode(Rect2I bounds)
    {
        Bounds = bounds;
    }

    public bool IsLeaf => LeftChild == null && RightChild == null;
}
