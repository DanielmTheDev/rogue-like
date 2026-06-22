using RogueLike.Code.View.Entities;
using RogueLike.Code.Items;
using RogueLike.Code.Domain.Actors;
using RogueLike.Code.Domain.Combat;
using RogueLike.Code.Domain.Flow;

namespace RogueLike.Code.View.Items.Consumables;

/// <summary>
/// A specific item that heals the player when walked over.
/// </summary>
public partial class HealingPotion : ItemController
{
    [Godot.Export] public int HealAmount { get; set; } = 5;

    public override void Initialize(ItemManager itemManager, Godot.Vector2I position)
    {
        ItemName = "Healing Potion";
        base.Initialize(itemManager, position);
    }

    public override bool Use(IActor actor)
    {
        if (actor is ICombatant combatant)
        {
            combatant.Health.Heal(HealAmount);
            GameLog.Instance.Log($"[color=green]You drink the Healing Potion and recover {HealAmount} HP![/color]");
            return true;
        }
        return false;
    }
}
