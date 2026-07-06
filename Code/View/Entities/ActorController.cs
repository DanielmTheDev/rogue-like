using System;
using Godot;
using RogueLike.Code.View.Audio;
using RogueLike.Domain.Common;
using RogueLike.Domain.Combat;
using RogueLike.Domain.Actors;

using RogueLike.Domain.Flow;

namespace RogueLike.Code.View.Entities;

/// <summary>
/// Base View for all grid actors that can engage in combat. Holds a pure domain <see cref="Actor"/>
/// (created by the subclass) and forwards the <see cref="ICombatant"/> contract to it; it renders the
/// actor's domain events (health bar, hit/death SFX, node cleanup). No combat rules live here.
/// </summary>
public abstract partial class ActorController : Node2D, ICombatant
{
    protected ActorRegistry _actorRegistry;

    /// <summary>The pure domain actor this View renders. Subclasses create + expose it.</summary>
    protected abstract Actor Actor { get; }

    public string DisplayName => Name.ToString();

    public Health Health => Actor.Health;
    public int AttackDamage => Actor.AttackDamage;

    /// <summary>Domain event for decoupled UI: fired on every health change with (current, max).</summary>
    public event Action<int, int> OnHealthChanged;

    // Abstract properties that specific actors must implement
    public abstract GridPos GridPosition { get; }
    public abstract bool IsPlayer { get; }

    [Export] public ProgressBar HealthBar { get; set; }
    [Export] public int BaseHealth { get; set; } = 10;
    [Export] public int BaseAttackDamage { get; set; } = 2;

    /// <summary>Base initialization. Captures the registry; subclasses create their Actor + call <see cref="ObserveActor"/>.</summary>
    public virtual void InitializeBase(ActorRegistry actorRegistry) => _actorRegistry = actorRegistry;

    public void ReceiveDamage(Damage damage)
    {
        if (Actor.Health.IsDead) return;
        if (damage.Amount > 0) SfxPlayer.Instance?.Play(Sfx.Hit);
        Actor.ReceiveDamage(damage); // raises OnHealthChanged (bar) and, on death, OnDied (Die)
    }

    public void Heal(int amount) => Actor.Heal(amount);

    public void IncreaseMaxHp(int amount) => Actor.IncreaseMaxHp(amount);

    public bool Attack(ICombatant defender) => Actor.Attack(defender);

    public virtual void Die()
    {
        SfxPlayer.Instance?.Play(Sfx.Death);
        GameLog.Instance.LogDeath(DisplayName);
        _actorRegistry?.UnregisterActor(this);
        QueueFree();
    }

    /// <summary>Route the (already-created) Actor's domain events to the View. Call once, at init.</summary>
    protected void ObserveActor()
    {
        Actor.OnHealthChanged += HandleHealthChanged;
        Actor.OnDied += Die;
        SyncHealthBar();
    }

    private void HandleHealthChanged(int current, int max)
    {
        OnHealthChanged?.Invoke(current, max);
        SyncHealthBar();
    }

    private void SyncHealthBar()
    {
        if (HealthBar == null) return;
        HealthBar.MaxValue = Actor.Health.Max;
        HealthBar.Value = Actor.Health.Current;
    }
}
