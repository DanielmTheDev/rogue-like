using RogueLike.Code.Domain.Common;
using RogueLike.Code.View.Entities;
using RogueLike.Code.Domain.Actors;
using RogueLike.Code.Domain.Grid;

namespace RogueLike.Code.View.Enemies;
/// <summary>
/// Godot node representing the enemy visually.
/// </summary>
public partial class EnemyController : ActorController
{
    private EnemyAI _ai;

    public override GridPos GridPosition => _ai?.GridPosition ?? GridPos.Origin;
    public override bool IsPlayer => false;
    public override int AttackDamage => BaseAttackDamage;

    public void Initialize(DungeonGrid grid, ActorRegistry actorRegistry, GridPos startPos)
    {
        InitializeBase(actorRegistry);

        _ai = new EnemyAI(this, grid, actorRegistry, startPos);
        actorRegistry.RegisterActor(this);
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
