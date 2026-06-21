using RogueLike.Code.Domain.Common;

namespace RogueLike.Code.Grid.FOV;

/// <summary>
/// A pure C# map overlay tracking the visibility state of every cell.
/// Separated from DungeonGrid to maintain single-responsibility.
/// </summary>
public class FovMap
{
    private readonly VisibilityState[,] _states;
    public readonly int Width;
    public readonly int Height;

    public FovMap(int width, int height)
    {
        Width = width;
        Height = height;
        _states = new VisibilityState[width, height];

        // Defaults to Unexplored strictly due to C# default enum behavior = 0.
    }

    public VisibilityState GetVisibility(GridPos pos)
    {
        if (pos.X < 0 || pos.X >= Width || pos.Y < 0 || pos.Y >= Height)
            return VisibilityState.Unexplored;

        return _states[pos.X, pos.Y];
    }

    public void SetVisibility(GridPos pos, VisibilityState state)
    {
        if (pos.X >= 0 && pos.X < Width && pos.Y >= 0 && pos.Y < Height)
        {
            _states[pos.X, pos.Y] = state;
        }
    }

    /// <summary>
    /// Resets all currently visible tiles to Explored. Call this before computing a new FOV.
    /// </summary>
    public void ResetVisible()
    {
        for (var x = 0; x < Width; x++)
        {
            for (var y = 0; y < Height; y++)
            {
                if (_states[x, y] == VisibilityState.Visible)
                {
                    _states[x, y] = VisibilityState.Explored;
                }
            }
        }
    }
}
