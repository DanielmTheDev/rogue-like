using Godot;
using RogueLike.Grid;
using RogueLike.Player;
using RogueLike.Rendering;

namespace RogueLike;

/// <summary>
/// Main scene controller. Creates the grid, renders the tilemap,
/// and spawns the player at the center of the map.
/// </summary>
public partial class Main : Node2D
{
    private const int GridWidth = 20;
    private const int GridHeight = 15;
    private const int TilePixelSize = 32;

    private DungeonGrid _gridMap;

    public override void _Ready()
    {
        _gridMap = new DungeonGrid(GridWidth, GridHeight, TilePixelSize);

        SetupTileMap();
        SetupPlayer();
    }

    private void SetupTileMap()
    {
        var tileMap = GetNode<DungeonTileMap>("DungeonTileMap");
        tileMap.Render(_gridMap);
    }

    private void SetupPlayer()
    {
        var player = GetNode<PlayerController>("Player");
        var center = new Vector2I(GridWidth / 2, GridHeight / 2);
        player.Initialize(_gridMap, center);
    }
}
