using Godot;

namespace RogueLike.Code.Items;

/// <summary>
/// Interface for all floor items.
/// </summary>
public interface IItem
{
    string DisplayName { get; }
    Vector2I GridPosition { get; }
    
    /// <summary>
    /// Triggered when an actor walks over the item.
    /// Returns true if the item should be removed from the world.
    /// </summary>
    bool ProcessPickup(Entities.IActor actor);
}
