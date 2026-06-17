using Godot;

namespace RogueLike.Code.Items;

/// <summary>
/// Interface for all floor items.
/// Phase 2: Items can now be picked up and stored in inventory.
/// </summary>
public interface IItem
{
    string DisplayName { get; }
    Vector2I GridPosition { get; }
    bool IsConsumable { get; } // Should item disappear after use?
    
    /// <summary>
    /// Can this item be picked up by the given actor?
    /// </summary>
    bool CanPickup(Entities.IActor actor);
    
    /// <summary>
    /// Called when the item is successfully picked up.
    /// Use for logging or effects.
    /// </summary>
    void OnPickup(Entities.IActor actor);
    
    /// <summary>
    /// Use the item from inventory.
    /// Returns true if the item was successfully used.
    /// </summary>
    bool Use(Entities.IActor actor);

    /// <summary>
    /// Attempts to pick this item up into the given inventory. Returns true if it was taken —
    /// the caller then unregisters it from the floor. Returns false (item stays on the floor)
    /// if it can't be picked up, there is no inventory, or the inventory is full.
    /// </summary>
    // TRANSITIONAL (DDD Phase 3): the item should not reach into the actor's inventory aggregate.
    // Target: Player.TryPickup(item) — the actor owns the acquire; the item keeps only CanPickup/OnPickup.
    bool TryPickup(Entities.IActor actor, Player.Inventory inventory)
    {
        if (!CanPickup(actor)) return false;
        if (inventory == null) return false;
        if (!inventory.AddItem(this))
        {
            Services.GameLog.Instance.Log("[color=orange]Your inventory is full![/color]");
            return false;
        }
        OnPickup(actor);
        return true;
    }
}
