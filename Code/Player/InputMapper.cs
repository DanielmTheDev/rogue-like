using System.Collections.Generic;
using Godot;

namespace RogueLike.Code.Player;

/// <summary>
/// Static utility to map Godot Input strings to grid vector directions.
/// </summary>
public static class InputMapper
{
    private static readonly Dictionary<string, Vector2I> DirectionMap = new()
    {
        { "move_up", Vector2I.Up },
        { "move_down", Vector2I.Down },
        { "move_left", Vector2I.Left },
        { "move_right", Vector2I.Right },
        { "move_h", Vector2I.Left },
        { "move_j", Vector2I.Down },
        { "move_k", Vector2I.Up },
        { "move_l", Vector2I.Right },
        { "move_up_left", new Vector2I(-1, -1) },
        { "move_up_right", new Vector2I(1, -1) },
        { "move_down_left", new Vector2I(-1, 1) },
        { "move_down_right", new Vector2I(1, 1) }
    };

    /// <summary>
    /// Checks the input event against all known direction mappings.
    /// Returns the Vector2I direction if pressed, otherwise Vector2I.Zero.
    /// </summary>
    public static Vector2I GetDirection(InputEvent @event)
    {
        foreach (var mapping in DirectionMap)
        {
            if (@event.IsActionPressed(mapping.Key))
            {
                return mapping.Value;
            }
        }

        return Vector2I.Zero;
    }
}
