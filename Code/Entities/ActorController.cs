using Godot;
using RogueLike.Code.Entities.Combat;

namespace RogueLike.Code.Entities;

/// <summary>
/// Base class for all Grid Actors that can engage in combat.
/// Centralizes Health and grid registration logic.
/// </summary>
public abstract partial class ActorController : Node2D, ICombatant
{
    protected EntityManager _entityManager;

    public HealthController Health { get; protected set; }

    // Abstract properties that specific actors must implement
    public abstract Vector2I GridPosition { get; }
    public abstract bool IsPlayer { get; }
    public abstract int AttackDamage { get; }

    /// <summary>
    /// Base initialization. Sets up health and UI.
    /// </summary>
    public virtual void InitializeBase(EntityManager entityManager, int initialHealth)
    {
        _entityManager = entityManager;
        Health = new HealthController(initialHealth);
        Health.OnDied += Die;
        
        SetupHealthUI();
    }

    protected void SetupHealthUI()
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

    public virtual void Die()
    {
        _entityManager?.UnregisterActor(this);
        QueueFree();
    }
}
