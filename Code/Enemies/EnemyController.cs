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
        SyncPosition(grid);
    }

    public void TakeTurn(DungeonGrid grid)
    {
        if (_ai == null) return;
        
        _ai.TakeTurn();
        SyncPosition(grid);
    }

    private void SyncPosition(DungeonGrid grid)
    {
        Position = grid.GridToWorld(GridPosition);
    }
}
