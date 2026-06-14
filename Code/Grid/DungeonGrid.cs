using Godot;

namespace RogueLike.Code.Grid;

/// <summary>
/// Pure data representation of the dungeon grid.
/// No Godot Node dependency — easily testable.
/// </summary>
public class DungeonGrid
{
    public Vector2I Size { get; }
    public int TileSize { get; }

    private readonly CellType[,] _cells;

    public DungeonGrid(int width, int height, int tileSize = 32)
    {
        Size = new Vector2I(width, height);
        TileSize = tileSize;
        _cells = new CellType[width, height];
        
        FillWith(CellType.Floor);
    }

    /// <summary>
    /// Returns true if the coordinate is within the grid bounds.
    /// </summary>
    public bool IsInBounds(Vector2I coord)
    {
        return coord.X >= 0
               && coord.Y >= 0
               && coord.X < Size.X
               && coord.Y < Size.Y;
    }

    /// <summary>
    /// Returns true if the coordinate is in bounds and the cell is walkable.
    /// </summary>
    public bool IsWalkable(Vector2I coord)
    {
        if (!IsInBounds(coord))
            return false;

        return _cells[coord.X, coord.Y] == CellType.Floor;
    }

    /// <summary>
    /// True if moving <paramref name="dir"/> from <paramref name="from"/> is a diagonal step
    /// that cuts a wall corner. A diagonal is only traversable when both orthogonally-adjacent
    /// cells are walkable. Cardinal directions are never corner cuts.
    /// </summary>
    public bool IsDiagonalCornerCut(Vector2I from, Vector2I dir)
    {
        if (dir.X == 0 || dir.Y == 0)
            return false;

        return !IsWalkable(from + new Vector2I(dir.X, 0))
            || !IsWalkable(from + new Vector2I(0, dir.Y));
    }

    /// <summary>
    /// Converts a grid coordinate to the world-space center of that tile.
    /// </summary>
    public Vector2 GridToWorld(Vector2I coord)
    {
        var halfTile = TileSize / 2f;
        return new Vector2(
            coord.X * TileSize + halfTile,
            coord.Y * TileSize + halfTile
        );
    }

    /// <summary>
    /// Converts a world-space position to the corresponding grid coordinate.
    /// </summary>
    public Vector2I WorldToGrid(Vector2 worldPos)
    {
        return new Vector2I(
            (int)(worldPos.X / TileSize),
            (int)(worldPos.Y / TileSize)
        );
    }

    /// <summary>
    /// Sets the cell type at the given coordinate.
    /// </summary>
    public void SetCell(Vector2I coord, CellType type)
    {
        if (!IsInBounds(coord))
            return;

        _cells[coord.X, coord.Y] = type;
    }

    /// <summary>
    /// Gets the cell type at the given coordinate.
    /// Returns Wall for out-of-bounds coordinates.
    /// </summary>
    public CellType GetCell(Vector2I coord)
    {
        if (!IsInBounds(coord))
            return CellType.Wall;

        return _cells[coord.X, coord.Y];
    }

    private void FillWith(CellType type)
    {
        for (var x = 0; x < Size.X; x++)
        {
            for (var y = 0; y < Size.Y; y++)
            {
                _cells[x, y] = type;
            }
        }
    }
}
