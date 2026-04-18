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
}
