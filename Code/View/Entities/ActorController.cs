using System;
using Godot;
using RogueLike.Code.View.Audio;
using RogueLike.Domain.Common;
using RogueLike.Domain.Combat;
using RogueLike.Domain.Actors;

using RogueLike.Domain.Flow;

namespace RogueLike.Code.View.Entities;

/// <summary>
/// Base class for all Grid Actors that can engage in combat.
/// Owns the actor's <see cref="Health"/> value object and grid registration logic; combat verbs
/// (<see cref="ReceiveDamage"/>/<see cref="Heal"/>) mutate the owned health and raise its events.
/// </summary>
public abstract partial class ActorController : Node2D, ICombatant
{
    protected ActorRegistry _actorRegistry;

    public string DisplayName => Name.ToString();

    public Health Health { get; private set; }

    /// <summary>Domain event for decoupled UI: fired on every health change with (current, max).</summary>
    public event Action<int, int> OnHealthChanged;

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
    public virtual void InitializeBase(ActorRegistry actorRegistry)
    {
        _actorRegistry = actorRegistry;
        InitializeHealth();
    }

    public void ReceiveDamage(Damage damage)
    {
        if (Health.IsDead) return;
        Health = Health.TakeDamage(damage.Amount);
        OnHealthChanged?.Invoke(Health.Current, Health.Max);
        SyncHealthBar();
        if (damage.Amount > 0) SfxPlayer.Instance?.Play(Sfx.Hit);
        if (Health.IsDead) Die();
    }

    public void Heal(int amount)
    {
        if (Health.IsDead) return;
        Health = Health.Heal(amount);
        OnHealthChanged?.Invoke(Health.Current, Health.Max);
        SyncHealthBar();
    }

    public void IncreaseMaxHp(int amount)
    {
        Health = Health.WithIncreasedMax(amount);
        OnHealthChanged?.Invoke(Health.Current, Health.Max);
        SyncHealthBar();
    }

    public virtual void Die()
    {
        SfxPlayer.Instance?.Play(Sfx.Death);
        GameLog.Instance.LogDeath(DisplayName);
        _actorRegistry?.UnregisterActor(this);
        QueueFree();
    }

    /// <summary>(Re)sets health to full from <see cref="BaseHealth"/> and syncs the bar. Used on spawn and reset.</summary>
    protected void InitializeHealth()
    {
        Health = new Health(BaseHealth, BaseHealth);
        SyncHealthBar();
    }

    private void SyncHealthBar()
    {
        if (HealthBar == null) return;
        HealthBar.MaxValue = Health.Max;
        HealthBar.Value = Health.Current;
    }
}
