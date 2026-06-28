using Godot;
using RogueLike.Domain.Common;
using RogueLike.Domain.Grid;
using RogueLike.Domain.Grid.Generators;
using RogueLike.Code.View.Grid;
using RogueLike.Code.View.Items;
using RogueLike.Code.View.Player;
using RogueLike.Domain.Flow;
using RogueLike.Code.View.Entities;
using RogueLike.Domain.Grid.FOV;
using RogueLike.Domain.Actors;
using RogueLike.Code.View.Grid.FOV;
using RogueLike.Domain.Items;
using RogueLike.Domain.Loot;
using RogueLike.Code.View.Resources;
using System.Linq;

namespace RogueLike.Code.View;

/// <summary>
/// Main scene controller. Creates the grid, renders the tilemap,
/// and spawns the player at the center of the map.
/// </summary>
public partial class Main : Node2D
{
    [Export] public LevelSettings LevelSettings { get; set; }

    private const int GridWidth = 50;
    private const int GridHeight = 50;

    private DungeonGrid _gridMap;
    private ActorRegistry _actorRegistry;
    private NodeRegistry _nodeRegistry;
    private TurnManager _turnManager;
    private FloorItems _floorItems;

    private FovMap _fovMap;
    private IFovAlgorithm _fovAlgorithm;
    private FovTileMap _fovTileMap;
    private UI.MinimapController _minimap;

    private System.Collections.Generic.List<GridRect> _rooms;
    private int _dungeonLevel = 1;

    // One loot RNG stream + table for the whole run (seeded from MapSeed); the table's
    // randomness is an injected collaborator and advances continuously across floors.
    private IRng _lootRng;
    private WeaponLootTable _lootTable;

    public override void _Ready()
    {
        _gridMap = new DungeonGrid(GridWidth, GridHeight);
        _actorRegistry = new ActorRegistry();
        _nodeRegistry = new NodeRegistry();
        _floorItems = new FloorItems();
        _fovMap = new FovMap(GridWidth, GridHeight);
        _fovAlgorithm = new Raycaster();
        _turnManager = new TurnManager();
        _turnManager.OnTurnChanged += OnTurnChanged;

        var player = GetNode<PlayerController>("Player");
        player.Initialize(this, _turnManager, _floorItems);

        InitLoot();
        SetupFovTileMap();
        SetupLevel();

        // Initialize UIs after player is fully initialized
        var inventoryUI = GetNode<UI.InventoryUI>("InventoryUI/InventoryControl");
        inventoryUI.Initialize(player.Inventory);
        var expUI = GetNode<UI.ExperienceUI>("ExperienceUI/ExperienceControl");
        expUI.Initialize(player.Experience);
        var weaponUI = GetNode<UI.WeaponUI>("WeaponUI/WeaponControl");
        weaponUI.Initialize(player.Loadout);
        _minimap = GetNode<UI.MinimapController>("MinimapUI/MinimapController");
        _minimap.Initialize(_gridMap, _fovMap, player);
    }

    private void SetupLevel()
    {
        // 1. Generate map layout (fixed seed if LevelSettings supplies one, else random)
        int? seed = LevelSettings?.MapSeed >= 0
            ? LevelSettings.MapSeed
            : null;
        _rooms = BspDungeonGenerator.Generate(_gridMap, seed);
        SetupTileMap();

        // 2. Place player
        var player = GetNode<PlayerController>("Player");
        Spawner.PlacePlayerOnLevel(player, _rooms[0], _gridMap, _actorRegistry, _fovMap, _nodeRegistry);

        // 3. Spawn entities
        var goblinScene = GD.Load<PackedScene>("res://Scenes/Enemy.tscn");
        var archerScene = GD.Load<PackedScene>("res://Scenes/Archer.tscn");
        var potionScene = GD.Load<PackedScene>("res://Scenes/HealingPotion.tscn");
        var stairsScene = GD.Load<PackedScene>("res://Scenes/StairsDown.tscn");
        var swordScene = GD.Load<PackedScene>("res://Scenes/Sword.tscn");

        Spawner.SpawnEnemies(this, goblinScene, archerScene, _rooms, _gridMap, _actorRegistry, _dungeonLevel,
            LevelSettings);
        Spawner.SpawnPotions(this, potionScene, _rooms, _gridMap, _floorItems);
        // Per-floor weapon loot: deeper floors drop more, and skew to higher tiers.
        Spawner.SpawnFloorLoot(this, swordScene, _rooms, _gridMap, _floorItems, _lootTable, _dungeonLevel, _lootRng);
        Spawner.SpawnStairs(this, stairsScene, _rooms.Last(), _nodeRegistry, _gridMap);

        // 4. Initial FOV Compute
        UpdateFov();
    }

    private void InitLoot()
    {
        int? seed = LevelSettings?.MapSeed >= 0 ? LevelSettings.MapSeed : null;
        _lootRng = new SystemRng(seed);
        _lootTable = new WeaponLootTable(BuildLootConfig(), _lootRng);
    }

    private LootTableConfig BuildLootConfig()
    {
        if (LevelSettings is null)
            return LootTableConfig.Default;

        return new LootTableConfig(
            new Probability(LevelSettings.BaseDropChance), new Probability(LevelSettings.DropChancePerFloor),
            new Probability(LevelSettings.MaxDropChance), new Probability(LevelSettings.UpgradeBaseChance),
            new Probability(LevelSettings.UpgradePerFloor), new Probability(LevelSettings.MaxUpgradeChance),
            LevelSettings.MaxWeaponTier);
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
        _minimap?.Refresh();

        // Sync visibility of all non-player actors
        // Use ToList() snapshot to avoid "Collection was modified" if entities change during sync
        foreach (var actor in _actorRegistry.AllActors.ToList())
        {
            if (actor is ActorController actorNode && !actor.IsPlayer)
            {
                var vis = _fovMap.GetVisibility(actor.GridPosition);
                actorNode.Visible = vis == VisibilityState.Visible;
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
        foreach (var actor in _actorRegistry.AllActors.ToList())
        {
            if (actor is Enemies.EnemyController goblin)
                goblin.TakeTurn(_gridMap, _fovMap);
            else if (actor is Enemies.ArcherController archer)
                archer.TakeTurn(_gridMap, _fovMap);
        }

        _turnManager.EndEnemyTurn();
    }

    private void SetupTileMap()
    {
        var tileMap = GetNode<DungeonTileMap>("DungeonTileMap");
        tileMap.Render(_gridMap);
    }

    public void DescendLevel()
    {
        _dungeonLevel++;
        GameLog.Instance.Log($"[color=purple]You descend to dungeon level {_dungeonLevel}...[/color]");

        // 1. Clean up old level entities (nodes will be children of Main)
        foreach (var node in GetChildren())
        {
            if (node is Enemies.EnemyController || node is ItemController || node is World.StairsController)
            {
                node.QueueFree();
            }
        }

        // 2. Clear registries
        _actorRegistry.ClearAll();
        _nodeRegistry.Clear();
        _floorItems.Clear();

        // 3. Generate and setup new level
        _gridMap = new DungeonGrid(GridWidth, GridHeight);
        _fovMap = new FovMap(GridWidth, GridHeight); // Reset FOV map
        SetupLevel();
        var player = GetNode<PlayerController>("Player");
        _minimap.Initialize(_gridMap, _fovMap, player);
    }

    public void RestartGame()
    {
        GameLog.Instance.Clear();
        GameLog.Instance.Log("[color=yellow]A new adventure begins...[/color]");

        var player = GetNode<PlayerController>("Player");
        player.Reset();
        InitLoot(); // fresh loot stream so a restart with a fixed seed reproduces

        // Reset the UI to reflect the player's new state
        var inventoryUI = GetNode<UI.InventoryUI>("InventoryUI/InventoryControl");
        inventoryUI.Initialize(player.Inventory);
        var expUI = GetNode<UI.ExperienceUI>("ExperienceUI/ExperienceControl");
        expUI.Initialize(player.Experience);

        _dungeonLevel = 0; // DescendLevel will increment this to 1
        DescendLevel();
    }
}