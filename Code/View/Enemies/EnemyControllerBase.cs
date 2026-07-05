using Godot;
using RogueLike.Domain.Common;
using RogueLike.Code.View.Entities;
using RogueLike.Domain.Actors;
using RogueLike.Domain.Grid;
using RogueLike.Domain.Grid.FOV;

namespace RogueLike.Code.View.Enemies;

/// <summary>
/// Shared Godot node base for enemies. Owns the enemy-only exports (`XpReward`, `SightRange`) and
/// the AI/position/turn plumbing; subclasses supply only their concrete AI via <see cref="CreateAi"/>.
/// The Player is deliberately NOT an <see cref="IEnemy"/>, so `XpReward` lives here — not on the
/// all-actor `ActorController`.
/// </summary>
public abstract partial class EnemyControllerBase : ActorController, IEnemy
{
    private EnemyAIBase _ai;
    // Per-level view context, captured once at Initialize (stable for the enemy's lifetime): the
    // grid for world-space projection, the FovMap for sprite fog-of-war. Neither feeds AI decisions.
    private DungeonGrid _grid;
    private FovMap _fovMap;

    [Export] public int XpReward { get; private set; } = 35;
    [Export] public int SightRange { get; set; } = 7;

    public override GridPos GridPosition => _ai?.GridPosition ?? GridPos.Origin;
    public override bool IsPlayer => false;
    public override int AttackDamage => BaseAttackDamage;

    public void Initialize(DungeonGrid grid, ActorRegistry actorRegistry, GridPos startPos, FovMap fovMap)
    {
        InitializeBase(actorRegistry);

        _grid = grid;
        _fovMap = fovMap;
        _ai = CreateAi(grid, actorRegistry, startPos);
        actorRegistry.RegisterActor(this);
        SyncWorldPosition(); // position only at spawn; Main.UpdateFov applies initial sprite visibility
    }

    public void TakeTurn()
    {
        if (_ai == null) return;

        _ai.TakeTurn();
        SyncPosition();
    }

    /// <summary>Builds the concrete AI (melee/ranged) for this enemy type.</summary>
    protected abstract EnemyAIBase CreateAi(DungeonGrid grid, ActorRegistry actorRegistry, GridPos startPos);

    private void SyncPosition()
    {
        SyncWorldPosition();
        // Post-move: refresh this sprite's fog-of-war for the tile it just stepped to (player FOV is
        // static during enemy turns, so a mover can step in/out of view).
        Visible = _fovMap.GetVisibility(GridPosition) == VisibilityState.Visible;
    }

    private void SyncWorldPosition() => Position = GridPosition.ToWorldCenter(_grid.TileSize);
}
