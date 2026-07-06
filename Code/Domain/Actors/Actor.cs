using System;
using RogueLike.Domain.Combat;
using RogueLike.Domain.Flow;

namespace RogueLike.Domain.Actors;

/// <summary>
/// Pure (Godot-free) base aggregate for a combat actor: owns its <see cref="Health"/> value object
/// and the combat verbs that mutate it, and raises domain events the View observes to render. Replaces
/// the old <c>ICombatant.TryAttack</c> default-interface-method + the combat rules that used to live on
/// the Godot <c>ActorController</c>. Position ownership folds in for enemies in Phase 3.3 (Enemy : Actor).
/// </summary>
public abstract class Actor
{
    public Health Health { get; private set; }

    public abstract string DisplayName { get; }
    public abstract int AttackDamage { get; }

    /// <summary>Fired on every health change with (current, max). The View maps it to its health bar.</summary>
    public event Action<int, int> OnHealthChanged;

    /// <summary>Fired when health reaches zero. The View reacts (SFX, corpse, QueueFree).</summary>
    public event Action OnDied;

    protected Actor(int maxHealth) => Health = new Health(maxHealth, maxHealth);

    /// <summary>
    /// This actor attacks the defender: deals its damage, logs the action, and—if the defender
    /// dies—invokes <see cref="OnKilled"/> on this attacker. Behavior lives with the data it mutates
    /// (rich domain). Returns false if there is no defender.
    /// </summary>
    public bool Attack(ICombatant defender)
    {
        if (defender == null) return false;
        var damage = new Damage(AttackDamage);
        defender.ReceiveDamage(damage);
        GameLog.Instance.LogCombat(DisplayName, defender.DisplayName, damage.Amount);
        if (defender.Health.IsDead) OnKilled(defender);
        return true;
    }

    /// <summary>Apply <paramref name="damage"/> to this actor's own health, raising its change/death events.</summary>
    public void ReceiveDamage(Damage damage)
    {
        if (Health.IsDead) return;
        Health = Health.TakeDamage(damage.Amount);
        OnHealthChanged?.Invoke(Health.Current, Health.Max);
        if (Health.IsDead) OnDied?.Invoke();
    }

    /// <summary>Restore HP (clamped to Max; the dead cannot be healed), raising the change event.</summary>
    public void Heal(int amount)
    {
        if (Health.IsDead) return;
        Health = Health.Heal(amount);
        OnHealthChanged?.Invoke(Health.Current, Health.Max);
    }

    /// <summary>Raise Max HP and heal to the new full, raising the change event.</summary>
    public void IncreaseMaxHp(int amount)
    {
        Health = Health.WithIncreasedMax(amount);
        OnHealthChanged?.Invoke(Health.Current, Health.Max);
    }

    /// <summary>Reset to full health at <paramref name="maxHealth"/> (revives the dead); raises the change event.</summary>
    public void ResetHealth(int maxHealth)
    {
        Health = new Health(maxHealth, maxHealth);
        OnHealthChanged?.Invoke(Health.Current, Health.Max);
    }

    /// <summary>
    /// Reaction hook invoked on this actor when its attack kills <paramref name="victim"/>.
    /// Default: no reaction. The Player overrides it to gain XP.
    /// </summary>
    protected virtual void OnKilled(ICombatant victim) { }
}
