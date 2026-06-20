using System;
using System.Collections.Generic;

namespace RogueLike.Code.Domain.Common;

/// <summary>
/// Value object for a single grid step. Immutable, equality-by-value, and closed: the constructor
/// is private, so the only ways to obtain a Direction are the named statics, <see cref="FromDelta"/>
/// (which validates), or <c>default</c> (== <see cref="None"/>). Each component is constrained to
/// {-1, 0, 1}, so an invalid Direction is unconstructable. Godot-free; the view edge converts
/// to/from <c>Vector2I</c>. Screen/grid convention: Y grows downward, so Up is (0, -1).
/// </summary>
public readonly record struct Direction
{
    public int Dx { get; }
    public int Dy { get; }

    private Direction(int dx, int dy)
    {
        Dx = dx;
        Dy = dy;
    }

    /// <summary>No movement (also the value of <c>default(Direction)</c>).</summary>
    public static readonly Direction None = new(0, 0);

    public static readonly Direction Up = new(0, -1);
    public static readonly Direction Down = new(0, 1);
    public static readonly Direction Left = new(-1, 0);
    public static readonly Direction Right = new(1, 0);
    public static readonly Direction UpLeft = new(-1, -1);
    public static readonly Direction UpRight = new(1, -1);
    public static readonly Direction DownLeft = new(-1, 1);
    public static readonly Direction DownRight = new(1, 1);

    public bool IsZero => Dx == 0 && Dy == 0;
    public bool IsDiagonal => Dx != 0 && Dy != 0;

    /// <summary>The eight non-zero neighbour directions (cardinals + diagonals).</summary>
    public static IReadOnlyList<Direction> AllEight { get; } =
    [
        Up, Down, Left, Right, UpLeft, UpRight, DownLeft, DownRight
    ];

    /// <summary>
    /// Builds a Direction from raw axis deltas. Each delta must already be a unit step
    /// (-1, 0 or 1); throws otherwise. Use with <c>Math.Sign</c> to reduce arbitrary deltas first.
    /// </summary>
    public static Direction FromDelta(int dx, int dy)
    {
        if (dx is < -1 or > 1) throw new ArgumentOutOfRangeException(nameof(dx), "Direction component must be -1, 0 or 1.");
        return dy is < -1 or > 1
            ? throw new ArgumentOutOfRangeException(nameof(dy), "Direction component must be -1, 0 or 1.")
            : new(dx, dy);
    }
}
