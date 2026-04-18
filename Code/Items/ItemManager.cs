using System.Collections.Generic;
using System.Linq;
using Godot;

namespace RogueLike.Code.Items;

/// <summary>
/// Pure C# manager tracking items on the grid.
/// </summary>
public class ItemManager
{
    private readonly List<IItem> _items = new();
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

    /// <summary>
    /// Checks if there is an item at the given position and processes it.
    /// </summary>
    public void CheckForPickup(Vector2I position, Entities.IActor actor)
    {
        var item = _items.FirstOrDefault(i => i.GridPosition == position);
        if (item != null)
        {
            bool shouldRemove = item.ProcessPickup(actor);
            if (shouldRemove)
            {
                UnregisterItem(item);
            }
        }
    }
}
