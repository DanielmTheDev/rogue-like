using Godot;
using RogueLike.Code.Entities;
using RogueLike.Code.Entities.Combat;
using RogueLike.Code.Grid;

namespace RogueLike.Code.Enemies;
/// <summary>
/// Godot node representing the enemy visually.
/// </summary>
public partial class EnemyController : Node2D, ICombatant
{
    private EnemyAI _ai;
    private EntityManager _entityManager;

    public Vector2I GridPosition => _ai?.GridPosition ?? Vector2I.Zero;
    public bool IsPlayer => false;
    
    public HealthController Health { get; private set; }
    public int AttackDamage => 2;

    public void Initialize(DungeonGrid grid, EntityManager entityManager, Vector2I startPos)
    {
        _entityManager = entityManager;
        Health = new HealthController(10); // 10 HP for an enemy
        Health.OnDied += Die;
        
        SetupHealthUI();
        
        _ai = new EnemyAI(this, grid, entityManager, startPos);
        entityManager.RegisterActor(this);
        SyncPosition(grid, null);
    }

    private void SetupHealthUI()
    {
        var bar = new ProgressBar
        {
            CustomMinimumSize = new Vector2(32, 6),
            Position = new Vector2(-16, -20), // Above the 32x32 sprite
            MaxValue = Health.MaxHp,
            Value = Health.CurrentHp,
            ShowPercentage = false
        };

        var bgStyle = new StyleBoxFlat { BgColor = Colors.DarkRed };
        var fgStyle = new StyleBoxFlat { BgColor = Colors.Green };
        
        bar.AddThemeStyleboxOverride("background", bgStyle);
        bar.AddThemeStyleboxOverride("fill", fgStyle);

        AddChild(bar);

        Health.OnHealthChanged += (current, max) => bar.Value = current;
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

    public void Die()
    {
        _entityManager.UnregisterActor(this);
        QueueFree();
    }
}
