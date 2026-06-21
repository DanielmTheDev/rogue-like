using Godot;
using RogueLike.Code.Domain.Common;
using RogueLike.Code.Entities;
using RogueLike.Code.Grid;
using RogueLike.Code.View;

namespace RogueLike.Code.Enemies;
/// <summary>
/// Godot node representing the enemy visually.
/// </summary>
public partial class EnemyController : ActorController
{
    private EnemyAI _ai;

    public override GridPos GridPosition => _ai?.GridPosition ?? GridPos.Origin;
    public override bool IsPlayer => false;
    public override int AttackDamage => BaseAttackDamage;

    public void Initialize(DungeonGrid grid, EntityManager entityManager, GridPos startPos)
    {
        InitializeBase(entityManager);

        _ai = new EnemyAI(this, grid, entityManager, startPos);
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
        Position = GridPosition.ToWorldCenter(grid.TileSize);

        if (fovMap != null)
        {
            var vis = fovMap.GetVisibility(GridPosition);
            Visible = vis == Grid.FOV.VisibilityState.Visible;
        }
    }
}
