using Godot;
using RogueLike.Code.Grid;
using RogueLike.Code.Player;
using RogueLike.Code.TurnContext;
using RogueLike.Code.Entities;
using RogueLike.Code.Grid.FOV;
using RogueLike.Code.Items;
using System.Linq;

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
    private ItemManager _itemManager;
    
    private FovMap _fovMap;
    private IFovAlgorithm _fovAlgorithm;
    private FovTileMap _fovTileMap;
    
    private System.Collections.Generic.List<Godot.Rect2I> _rooms;

    public override void _Ready()
    {
        _gridMap = new DungeonGrid(GridWidth, GridHeight, TilePixelSize);
        _rooms = Code.Grid.Generators.BspDungeonGenerator.Generate(_gridMap);
        
        _entityManager = new EntityManager();
        _itemManager = new ItemManager();
        
        _fovMap = new FovMap(GridWidth, GridHeight);
        _fovAlgorithm = new Raycaster();
        
        _turnManager = new TurnManager();
        _turnManager.OnTurnChanged += OnTurnChanged;

        SetupTileMap();
        SetupFovTileMap();
        
        var player = GetNode<PlayerController>("Player");
        Spawner.InitializePlayer(player, _rooms[0], _gridMap, _entityManager, _turnManager, _itemManager);
        
        // Initialize Inventory UI
        var inventoryUI = GetNode<UI.InventoryUI>("InventoryUI/InventoryControl");
        inventoryUI.Initialize(player.Inventory);
        
        var goblinScene = GD.Load<PackedScene>("res://Scenes/Enemy.tscn");
        var archerScene = GD.Load<PackedScene>("res://Scenes/Archer.tscn");
        var potionScene = GD.Load<PackedScene>("res://Scenes/HealingPotion.tscn");
        
        Spawner.SpawnEnemies(this, goblinScene, archerScene, _rooms, _gridMap, _entityManager);
        Spawner.SpawnPotions(this, potionScene, _rooms, _gridMap, _itemManager);
        
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

        // Sync visibility of all non-player actors
        // Use ToList() snapshot to avoid "Collection was modified" if entities change during sync
        foreach (var actor in _entityManager.AllActors.ToList())
        {
            if (actor is ActorController actorNode && !actor.IsPlayer)
            {
                var vis = _fovMap.GetVisibility(actor.GridPosition);
                actorNode.Visible = vis == Code.Grid.FOV.VisibilityState.Visible;
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
        // Iterate over a snapshot (ToList) because enemies (or the player) 
        // might die and unregister themselves during this loop.
        foreach (var actor in _entityManager.AllActors.ToList())
        {
            if (actor is Code.Enemies.EnemyController goblin)
                goblin.TakeTurn(_gridMap, _fovMap);
            else if (actor is Code.Enemies.ArcherController archer)
                archer.TakeTurn(_gridMap, _fovMap);
        }

        _turnManager.EndEnemyTurn();
    }

    private void SetupTileMap()
    {
        var tileMap = GetNode<DungeonTileMap>("DungeonTileMap");
        tileMap.Render(_gridMap);
    }
}
