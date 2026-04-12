using Godot;
using RogueLike.Code.Grid;
using RogueLike.Code.Player;
using RogueLike.Code.TurnContext;
using RogueLike.Code.Entities;
using RogueLike.Code.Grid.FOV;

namespace RogueLike.Code;

/// <summary>
/// Main scene controller. Creates the grid, renders the tilemap,
/// and spawns the player at the center of the map.
/// </summary>
public partial class Main : Node2D
{
    private const int GridWidth = 50;
    private const int GridHeight = 50;
    private const int TilePixelSize = 32;

    private DungeonGrid _gridMap;
    private EntityManager _entityManager;
    private TurnManager _turnManager;
    
    private FovMap _fovMap;
    private IFovAlgorithm _fovAlgorithm;
    private FovTileMap _fovTileMap;
    
    private System.Collections.Generic.List<Godot.Rect2I> _rooms;

    public override void _Ready()
    {
        _gridMap = new DungeonGrid(GridWidth, GridHeight, TilePixelSize);
        _rooms = Code.Grid.Generators.BspDungeonGenerator.Generate(_gridMap);
        
        _entityManager = new EntityManager();
        
        _fovMap = new FovMap(GridWidth, GridHeight);
        _fovAlgorithm = new Raycaster();
        
        _turnManager = new TurnManager();
        _turnManager.OnTurnChanged += OnTurnChanged;

        SetupTileMap();
        SetupFovTileMap();
        
        var player = GetNode<PlayerController>("Player");
        Spawner.InitializePlayer(player, _rooms[0], _gridMap, _entityManager, _turnManager);
        
        var enemyScene = GD.Load<PackedScene>("res://Scenes/Enemy.tscn");
        Spawner.SpawnEnemies(this, enemyScene, _rooms, _gridMap, _entityManager);
        
        // Initial FOV Compute
        UpdateFov();
    }

    private void SetupFovTileMap()
    {
        _fovTileMap = new FovTileMap();
        _fovTileMap.Initialize();
        AddChild(_fovTileMap);
    }

    private void UpdateFov()
    {
        var player = GetNode<PlayerController>("Player");
        _fovAlgorithm.ComputeFov(_fovMap, _gridMap, player.GridPosition, 6); // Radius 6
        _fovTileMap.Render(_fovMap);

        // Sync initial visibility of enemies immediately
        foreach (var actor in _entityManager.AllActors)
        {
            if (actor is Code.Enemies.EnemyController enemy)
            {
                var vis = _fovMap.GetVisibility(enemy.GridPosition);
                enemy.Visible = vis == Code.Grid.FOV.VisibilityState.Visible;
            }
        }
    }



    private void OnTurnChanged(TurnState newState)
    {
        if (newState == TurnState.Enemy)
        {
            UpdateFov(); // Calculate FOV exactly when player finishes stepping
            ProcessEnemyTurns();
        }
    }

    private void ProcessEnemyTurns()
    {
        // For each actor that is NOT the player, try taking a turn
        foreach (var actor in _entityManager.AllActors)
        {
            if (actor is Code.Enemies.EnemyController enemy)
            {
                enemy.TakeTurn(_gridMap, _fovMap);
            }
        }
        
        // Enemies finished, return control to player
        _turnManager.EndEnemyTurn();
    }

    private void SetupTileMap()
    {
        var tileMap = GetNode<DungeonTileMap>("DungeonTileMap");
        tileMap.Render(_gridMap);
    }


}
