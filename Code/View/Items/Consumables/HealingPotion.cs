using RogueLike.Domain.Items;
using RogueLike.Domain.Common;
using RogueLike.Domain.Actors;
using RogueLike.Domain.Combat;
using RogueLike.Domain.Flow;

namespace RogueLike.Code.View.Items.Consumables;

/// <summary>
/// A specific item that heals the player when walked over.
/// </summary>
public partial class HealingPotion : ItemController
{
    [Godot.Export] public int HealAmount { get; set; } = 5;

    public override void Initialize(FloorItems floorItems, GridPos position)
    {
        ItemName = "Healing Potion";
        base.Initialize(floorItems, position);
    }

    public override bool Use(IActor actor)
    {
        if (actor is ICombatant combatant)
        {
            combatant.Heal(HealAmount);
            GameLog.Instance.Log($"[color=green]You drink the Healing Potion and recover {HealAmount} HP![/color]");
            return true;
        }
        return false;
    }
}
