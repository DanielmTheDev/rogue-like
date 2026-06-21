using Godot;
using RogueLike.Code.Domain.Common;

namespace RogueLike.Code.View;

/// <summary>
/// The single bridge between Godot's <see cref="Vector2I"/>/<see cref="Vector2"/> and the
/// Godot-free domain <see cref="GridPos"/>. Lives on the view side because the domain must not
/// reference Godot types; callers at the Godot edge convert here rather than the domain reaching out.
/// Extension methods (not a bare static utility) so conversions read fluently off the value and
/// surface in IntelliSense — e.g. <c>coord.ToGridPos()</c>, <c>pos.ToWorldCenter(tileSize)</c>.
/// </summary>
public static class GridConversions
{
    /// <summary>Grid coordinate (column, row) → domain <see cref="GridPos"/>.</summary>
    public static GridPos ToGridPos(this Vector2I coord) => new(coord.X, coord.Y);

    /// <summary>Domain <see cref="GridPos"/> → grid coordinate (column, row).</summary>
    public static Vector2I ToVector2I(this GridPos pos) => new(pos.X, pos.Y);

    /// <summary>World-space center (pixels) of the tile at <paramref name="pos"/>.</summary>
    public static Vector2 ToWorldCenter(this GridPos pos, int tileSize)
    {
        var halfTile = tileSize / 2f;
        return new Vector2(pos.X * tileSize + halfTile, pos.Y * tileSize + halfTile);
    }

    /// <summary>The grid cell containing the world-space position <paramref name="world"/>.</summary>
    public static GridPos ToGridPos(this Vector2 world, int tileSize)
        => new((int)(world.X / tileSize), (int)(world.Y / tileSize));
}
