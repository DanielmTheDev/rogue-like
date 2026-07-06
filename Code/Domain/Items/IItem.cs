using RogueLike.Domain.Common;

namespace RogueLike.Domain.Items;

/// <summary>
/// Interface for all floor items. The acquire (equip vs store) is owned by the picker
/// (see <see cref="IItemPicker.TryPickup"/>); an item only says whether it can be taken
/// (<see cref="CanPickup"/>) and reacts once it is (<see cref="OnPickup"/>). A weapon opts into
/// equipping by also implementing <see cref="Equipment.IEquippable"/>.
/// </summary>
public interface IItem
{
    string DisplayName { get; }
    GridPos GridPosition { get; }
    bool IsConsumable { get; } // Should item disappear after use?

    /// <summary>Can this item be picked up right now?</summary>
    bool CanPickup();

    /// <summary>Called when the item is successfully picked up. Use for logging or effects.</summary>
    void OnPickup();

    /// <summary>
    /// Use the item from inventory.
    /// Returns true if the item was successfully used.
    /// </summary>
    bool Use(Actors.IActor actor);
}
