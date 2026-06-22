using System;

namespace RogueLike.Code.Domain.Common;

/// <summary>
/// Immutable axis-aligned integer rectangle on the grid (origin + size). Used for dungeon rooms
/// and BSP partitions — the Godot-free replacement for <c>Rect2I</c>. <see cref="End"/> is
/// exclusive (Position + size), matching <c>Rect2I.End</c>. Width/Height must be positive.
/// </summary>
public readonly record struct GridRect
{
    public GridPos Position { get; }
    public int Width { get; }
    public int Height { get; }

    public GridRect(GridPos position, int width, int height)
    {
        if (width <= 0)
            throw new ArgumentOutOfRangeException(nameof(width), "Width must be positive.");
        if (height <= 0)
            throw new ArgumentOutOfRangeException(nameof(height), "Height must be positive.");

        Position = position;
        Width = width;
        Height = height;
    }

    public GridRect(int x, int y, int width, int height) : this(new GridPos(x, y), width, height) { }

    public int X => Position.X;
    public int Y => Position.Y;

    /// <summary>Exclusive bottom-right corner (Position + size).</summary>
    public GridPos End => new(Position.X + Width, Position.Y + Height);

    /// <summary>Integer center cell of the rectangle.</summary>
    public GridPos Center => new(Position.X + Width / 2, Position.Y + Height / 2);
}
