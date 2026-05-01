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

    public void Clear()
    {
        // We only clear the list. The nodes themselves are children of Main and will be
        // QueueFree'd from there during a level change.
        _items.Clear();
    }

    /// <summary>
    /// Checks if there is an item at the given position and attempts to add it to inventory.
    /// If inventory is null or full, the item stays on the floor.
    /// </summary>
    public void CheckForPickup(Vector2I position, Entities.IActor actor, Player.Inventory inventory)
    {
        var item = _items.FirstOrDefault(i => i.GridPosition == position);
        if (item == null)
            return;

        if (!item.CanPickup(actor))
            return;

        if (inventory == null)
        {
            // No inventory system (for NPCs or future features)
            return;
        }

        if (inventory.AddItem(item))
        {
            item.OnPickup(actor);
            UnregisterItem(item);
            
            // Remove the visual node from the scene tree
            if (item is Godot.Node node)
            {
                node.QueueFree();
            }
        }
        else
        {
            Services.GameLog.Instance.Log("[color=orange]Your inventory is full![/color]");
        }
    }
}
