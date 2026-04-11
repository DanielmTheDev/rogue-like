using Godot;

namespace RogueLike.Code.Grid;

/// <summary>
/// Renders a DungeonGrid as a checkerboard pattern using Godot's TileMapLayer.
/// This is a thin visual wrapper — all data lives in DungeonGrid.
/// </summary>
public partial class DungeonTileMap : TileMapLayer
{
    // Atlas coordinates for the two checkerboard colors.
    // These reference tiles in the TileSet atlas.
    private static readonly Vector2I LightTile = new(0, 0);
    private static readonly Vector2I DarkTile = new(1, 0);

    private const int SourceId = 0;

    /// <summary>
    /// Populates the tilemap visuals from the given DungeonGrid data.
    /// </summary>
    public void Render(DungeonGrid gridMap)
    {
        Clear();

        for (var x = 0; x < gridMap.Size.X; x++)
        {
            for (var y = 0; y < gridMap.Size.Y; y++)
            {
                var coord = new Vector2I(x, y);
                var cellType = gridMap.GetCell(coord);

                if (cellType == CellType.Floor)
                {
                    var isLight = (x + y) % 2 == 0;
                    var atlasCoord = isLight ? LightTile : DarkTile;
                    SetCell(coord, SourceId, atlasCoord);
                }
                // Walls: no tile drawn (empty/void) for now
            }
        }
    }
}
