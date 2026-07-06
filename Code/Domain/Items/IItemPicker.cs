namespace RogueLike.Domain.Items;

/// <summary>
/// An actor that can acquire floor items. It owns the acquire decision (equip vs store) and its own
/// inventory/loadout, so <see cref="FloorItems"/> can delegate a pickup without depending on the
/// concrete actor type. Implemented by the <c>Player</c> aggregate.
/// </summary>
public interface IItemPicker
{
    /// <summary>Attempt to take <paramref name="item"/>. Returns true if taken (caller unregisters it).</summary>
    bool TryPickup(IItem item);
}
