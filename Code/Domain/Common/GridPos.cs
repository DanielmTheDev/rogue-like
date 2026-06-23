using System;

namespace RogueLike.Domain.Common;

/// <summary>
/// Value object for a position on the dungeon grid. Immutable, equality-by-value (so it works
/// directly as a dictionary/set key). Godot-free: the view edge converts to/from <c>Vector2I</c>
/// via <c>new GridPos(v.X, v.Y)</c> / <c>new Vector2I(p.X, p.Y)</c>. Bounds are the grid's concern,
/// not this type's, so any integer coordinate is valid.
/// </summary>
public readonly record struct GridPos(int X, int Y)
{
    public static readonly GridPos Origin = new(0, 0);

    /// <summary>The position one step away in the given direction.</summary>
    public GridPos Step(Direction d) => new(X + d.Dx, Y + d.Dy);

    /// <summary>4-directional (taxicab) distance.</summary>
    public int ManhattanTo(GridPos other) => Math.Abs(X - other.X) + Math.Abs(Y - other.Y);

    /// <summary>8-directional (chessboard) distance.</summary>
    public int ChebyshevTo(GridPos other) => Math.Max(Math.Abs(X - other.X), Math.Abs(Y - other.Y));

    /// <summary>The unit step (per-axis sign) pointing from this position towards <paramref name="other"/>.</summary>
    public Direction DirectionTo(GridPos other)
        => Direction.FromDelta(Math.Sign(other.X - X), Math.Sign(other.Y - Y));
}
