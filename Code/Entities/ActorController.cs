using Godot;
using RogueLike.Code.Domain.Common;
using RogueLike.Code.Domain.Combat;

using RogueLike.Code.Domain.Flow;

namespace RogueLike.Code.Entities;

/// <summary>
/// Base class for all Grid Actors that can engage in combat.
/// Centralizes Health and grid registration logic.
/// </summary>
public abstract partial class ActorController : Node2D, ICombatant
{
    protected EntityManager _entityManager;

    public string DisplayName => Name.ToString();

    public HealthController Health { get; protected set; }

    // Abstract properties that specific actors must implement
    public abstract GridPos GridPosition { get; }
    public abstract bool IsPlayer { get; }
    public abstract int AttackDamage { get; }

    [Export] public ProgressBar HealthBar { get; set; }
    [Export] public int BaseHealth { get; set; } = 10;
    [Export] public int BaseAttackDamage { get; set; } = 2;
    [Export] public int XpReward { get; private set; } = 35;

    /// <summary>
    /// Base initialization. Sets up health and UI mapping.
    /// </summary>
    public virtual void InitializeBase(EntityManager entityManager)
    {
        _entityManager = entityManager;
        Health = new HealthController(BaseHealth);
        Health.OnDied += Die;

        // Link to Godot inspector node if exists
        if (HealthBar != null)
        {
            HealthBar.MaxValue = Health.MaxHp;
            HealthBar.Value = Health.CurrentHp;
            Health.OnHealthChanged += (current, max) => HealthBar.Value = current;
        }
    }

    public virtual void Die()
    {
        GameLog.Instance.LogDeath(DisplayName);
        _entityManager?.UnregisterActor(this);
        QueueFree();
    }
}
