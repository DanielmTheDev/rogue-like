using Godot;
using RogueLike.Code.Domain.Common;
using RogueLike.Code.View.Entities;
using RogueLike.Code.Entities;
using RogueLike.Code.Domain.Actors;
using RogueLike.Code.Grid;

namespace RogueLike.Code.View.Enemies;

/// <summary>
/// Godot node representing the Skeleton Archer visually.
/// Delegates all decision-making to ArcherAI.
/// </summary>
public partial class ArcherController : ActorController
{
    private ArcherAI _ai;

    [Export] public int Range { get; set; } = 5;

    public override GridPos GridPosition => _ai?.GridPosition ?? GridPos.Origin;
    public override bool IsPlayer => false;
    public override int AttackDamage => BaseAttackDamage;

    public void Initialize(DungeonGrid grid, EntityManager entityManager, GridPos startPos)
    {
        InitializeBase(entityManager);

        _ai = new ArcherAI(this, grid, entityManager, startPos, Range);
        entityManager.RegisterActor(this);
        SyncPosition(grid, null);
    }

    public void TakeTurn(DungeonGrid grid, Domain.Grid.FOV.FovMap fovMap)
    {
        if (_ai == null) return;

        _ai.TakeTurn(fovMap);
        SyncPosition(grid, fovMap);
    }

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
