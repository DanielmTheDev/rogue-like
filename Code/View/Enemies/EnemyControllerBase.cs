using Godot;
using RogueLike.Domain.Common;
using RogueLike.Code.View.Entities;
using RogueLike.Domain.Actors;
using RogueLike.Domain.Grid;

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

    [Export] public int XpReward { get; private set; } = 35;
    [Export] public int SightRange { get; set; } = 7;

    public override GridPos GridPosition => _ai?.GridPosition ?? GridPos.Origin;
    public override bool IsPlayer => false;
    public override int AttackDamage => BaseAttackDamage;

    public void Initialize(DungeonGrid grid, ActorRegistry actorRegistry, GridPos startPos)
    {
        InitializeBase(actorRegistry);

        _ai = CreateAi(grid, actorRegistry, startPos);
        actorRegistry.RegisterActor(this);
        SyncPosition(grid, null);
    }

    public void TakeTurn(DungeonGrid grid, Domain.Grid.FOV.FovMap fovMap)
    {
        if (_ai == null) return;

        _ai.TakeTurn();
        SyncPosition(grid, fovMap);
    }

    /// <summary>Builds the concrete AI (melee/ranged) for this enemy type.</summary>
    protected abstract EnemyAIBase CreateAi(DungeonGrid grid, ActorRegistry actorRegistry, GridPos startPos);

    private void SyncPosition(DungeonGrid grid, Domain.Grid.FOV.FovMap fovMap)
    {
        Position = GridPosition.ToWorldCenter(grid.TileSize);

        if (fovMap != null)
        {
            var vis = fovMap.GetVisibility(GridPosition);
            Visible = vis == Domain.Grid.FOV.VisibilityState.Visible;
        }
    }
}
