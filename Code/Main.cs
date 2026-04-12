using Godot;
using RogueLike.Code.Grid;
using RogueLike.Code.Player;
using RogueLike.Code.TurnContext;
using RogueLike.Code.Entities;

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

    public override void _Ready()
    {
        _gridMap = new DungeonGrid(GridWidth, GridHeight, TilePixelSize);
        Code.Grid.Generators.PillarArenaGenerator.Generate(_gridMap);
        
        _entityManager = new EntityManager();
        
        _turnManager = new TurnManager();
        _turnManager.OnTurnChanged += OnTurnChanged;

        SetupTileMap();
        SetupPlayer();
        SetupEnemies();
    }

    private void SetupEnemies()
    {
        var enemyScene = GD.Load<PackedScene>("res://Scenes/Enemy.tscn");
        var enemyNode = enemyScene.Instantiate<Code.Enemies.EnemyController>();
        
        // Spawn them slightly offset from the center
        var startPos = new Vector2I(GridWidth / 2 + 5, GridHeight / 2 + 5);
        enemyNode.Initialize(_gridMap, _entityManager, startPos);
        AddChild(enemyNode);
    }

    private void OnTurnChanged(TurnState newState)
    {
        if (newState == TurnState.Enemy)
        {
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
                enemy.TakeTurn(_gridMap);
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

    private void SetupPlayer()
    {
        var player = GetNode<PlayerController>("Player");
        var center = new Vector2I(GridWidth / 2, GridHeight / 2);
        player.Initialize(_gridMap, _entityManager, _turnManager, center);
    }
}
