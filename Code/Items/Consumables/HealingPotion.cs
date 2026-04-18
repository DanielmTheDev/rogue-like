using RogueLike.Code.Entities;
using RogueLike.Code.Entities.Combat;
using RogueLike.Code.Services;

namespace RogueLike.Code.Items.Consumables;

/// <summary>
/// A specific item that heals the player when walked over.
/// </summary>
public partial class HealingPotion : ItemController
{
    public int HealAmount { get; set; } = 5;

    public override void Initialize(ItemManager itemManager, Godot.Vector2I position)
    {
        ItemName = "Healing Potion";
        base.Initialize(itemManager, position);
        
        // Add visual sprite
        var sprite = new Godot.Sprite2D();
        sprite.Texture = Godot.GD.Load<Godot.Texture2D>("res://Assets/Potion/potion.png");
        AddChild(sprite);
    }

    public override bool ProcessPickup(IActor actor)
    {
        if (actor is ICombatant combatant)
        {
            combatant.Health.Heal(HealAmount);
            GameLog.Instance.Log($"[color=green]You drink the Healing Potion and recover {HealAmount} HP![/color]");
            QueueFree();
            return true;
        }
        return false;
    }
}
