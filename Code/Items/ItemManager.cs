using System.Collections.Generic;
using System.Linq;
using Godot;

namespace RogueLike.Code.Items;

/// <summary>
/// Pure C# manager tracking items on the grid.
/// </summary>
public class ItemManager
{
    private readonly List<IItem> _items = [];
    public IReadOnlyList<IItem> AllItems => _items;

    public void RegisterItem(IItem item)
    {
        if (!_items.Contains(item))
            _items.Add(item);
    }

    public void UnregisterItem(IItem item)
    {
        _items.Remove(item);
    }

    public void Clear()
    {
        // We only clear the list. The nodes themselves are children of Main and will be
        // QueueFree'd from there during a level change.
        _items.Clear();
    }

    /// <summary>
    /// If there is an item at the given position, delegates the pickup decision to the item
    /// (see <see cref="IItem.TryPickup"/>) and unregisters it from the floor if it was taken.
    /// </summary>
    public void CheckForPickup(Vector2I position, Domain.Actors.IActor actor, Domain.Items.Inventory inventory)
    {
        var item = _items.FirstOrDefault(i => i.GridPosition == position);
        if (item == null)
            return;

        if (item.TryPickup(actor, inventory))
            UnregisterItem(item);
    }
}
