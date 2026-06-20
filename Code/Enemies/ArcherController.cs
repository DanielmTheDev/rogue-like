using Godot;
using RogueLike.Code.Entities;
using RogueLike.Code.Grid;

namespace RogueLike.Code.Enemies;

/// <summary>
/// Godot node representing the Skeleton Archer visually.
/// Delegates all decision-making to ArcherAI.
/// </summary>
public partial class ArcherController : ActorController
{
    private ArcherAI _ai;

    [Export] public int Range { get; set; } = 5;

    public override Vector2I GridPosition => _ai?.GridPosition ?? Vector2I.Zero;
    public override bool IsPlayer => false;
    public override int AttackDamage => BaseAttackDamage;

    public void Initialize(DungeonGrid grid, EntityManager entityManager, Vector2I startPos)
    {
        InitializeBase(entityManager);

        _ai = new ArcherAI(this, grid, entityManager, startPos, Range);
        entityManager.RegisterActor(this);
        SyncPosition(grid, null);
    }

    public void TakeTurn(DungeonGrid grid, Grid.FOV.FovMap fovMap)
    {
        if (_ai == null) return;

        _ai.TakeTurn(fovMap);
        SyncPosition(grid, fovMap);
    }

    private void SyncPosition(DungeonGrid grid, Grid.FOV.FovMap fovMap)
    {
        Position = grid.GridToWorld(GridPosition);

        if (fovMap != null)
        {
            var vis = fovMap.GetVisibility(GridPosition);
            Visible = vis == Grid.FOV.VisibilityState.Visible;
        }
    }
}
