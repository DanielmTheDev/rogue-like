using Godot;
using RogueLike.Code.Entities;
using RogueLike.Code.Grid;
using RogueLike.Code.Pathfinding;

namespace RogueLike.Code.Enemies;
/// <summary>
/// Godot node representing the enemy visually.
/// </summary>
public partial class EnemyController : ActorController
{
    private EnemyAI _ai;

    public override Vector2I GridPosition => _ai?.GridPosition ?? Vector2I.Zero;
    public override bool IsPlayer => false;
    public override int AttackDamage => BaseAttackDamage;

    public void Initialize(DungeonGrid grid, EntityManager entityManager, Pathfinder pathfinder, Vector2I startPos)
    {
        InitializeBase(entityManager);

        _ai = new EnemyAI(this, grid, entityManager, pathfinder, startPos);
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
