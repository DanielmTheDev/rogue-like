using Godot;
using RogueLike.Code.Grid;
using RogueLike.Code.Player;
using RogueLike.Code.TurnContext;

namespace RogueLike.Code;

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
    private TurnManager _turnManager;

    public override void _Ready()
    {
        _gridMap = new DungeonGrid(GridWidth, GridHeight, TilePixelSize);
        
        _turnManager = new TurnManager();
        _turnManager.OnTurnChanged += OnTurnChanged;

        SetupTileMap();
        SetupPlayer();
    }

    private void OnTurnChanged(TurnState newState)
    {
        // Simple log for verification that the game loop is functioning.
        GD.Print($"Turn changed to: {newState}");
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
        player.Initialize(_gridMap, _turnManager, center);
    }
}
