using Godot;
using RogueLike.Code.Entities;
using RogueLike.Code.Grid;

namespace RogueLike.Code.Enemies;

/// <summary>
/// Godot node representing the enemy visually.
/// </summary>
public partial class EnemyController : Node2D, IActor
{
    private EnemyAI _ai;

    public Vector2I GridPosition => _ai?.GridPosition ?? Vector2I.Zero;
    public bool IsPlayer => false;

    public void Initialize(DungeonGrid grid, EntityManager entityManager, Vector2I startPos)
    {
        _ai = new EnemyAI(this, grid, entityManager, startPos);
        entityManager.RegisterActor(this);
        SyncPosition(grid, null);
    }

    public void TakeTurn(DungeonGrid grid, RogueLike.Code.Grid.FOV.FovMap fovMap)
    {
        if (_ai == null) return;
        
        _ai.TakeTurn();
        SyncPosition(grid, fovMap);
    }

    private void SyncPosition(DungeonGrid grid, RogueLike.Code.Grid.FOV.FovMap fovMap)
    {
        Position = grid.GridToWorld(GridPosition);
        
        if (fovMap != null)
        {
            var vis = fovMap.GetVisibility(GridPosition);
            Visible = vis == RogueLike.Code.Grid.FOV.VisibilityState.Visible;
        }
    }
}
