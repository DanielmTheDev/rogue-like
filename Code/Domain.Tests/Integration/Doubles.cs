using RogueLike.Domain.Actors;
using RogueLike.Domain.Combat;
using RogueLike.Domain.Common;
using RogueLike.Domain.Items;

namespace RogueLike.Domain.Tests.Integration;

// Lightweight domain doubles for the Godot-free integration layer. They implement the real
// production interfaces (ICombatant / IItem) and hold only state — all behavior is delegated to
// production value objects/services (Health, EnemyAI). Confined to the test project.

internal sealed class PlayerDouble(GridPos pos) : ICombatant
{
    public GridPos GridPosition { get; } = pos;
    public bool IsPlayer => true;
    public string DisplayName => "Player";
    public Health Health { get; private set; } = new(20, 20);
    public int AttackDamage => 5;
    public int XpReward => 0;
    public void Die() { }
    public void ReceiveDamage(Damage damage) => Health = Health.TakeDamage(damage.Amount);
    public void Heal(int amount) => Health = Health.Heal(amount);
}

internal sealed class EnemyDouble(GridPos start) : ICombatant
{
    // The AI owns the GridMover that actually holds position, so position delegates to it
    // (mirrors how EnemyController exposes GridPosition in the view).
    public EnemyAI Ai { get; set; }
    public GridPos GridPosition => Ai?.GridPosition ?? start;
    public bool IsPlayer => false;
    public string DisplayName => "Goblin";
    public Health Health { get; private set; } = new(10, 10);
    public int AttackDamage => 3;
    public int XpReward => 5;
    public void Die() { }
    public void ReceiveDamage(Damage damage) => Health = Health.TakeDamage(damage.Amount);
    public void Heal(int amount) => Health = Health.Heal(amount);
}

internal sealed class ItemDouble(GridPos pos) : IItem
{
    public string DisplayName => "Potion";
    public GridPos GridPosition { get; } = pos;
    public bool IsConsumable => true;
    public bool CanPickup(IActor actor) => true;
    public void OnPickup(IActor actor) { }
    public bool Use(IActor actor) => true;
}
