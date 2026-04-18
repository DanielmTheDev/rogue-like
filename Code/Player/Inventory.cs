using System;
using System.Collections.Generic;
using System.Linq;
using RogueLike.Code.Items;

namespace RogueLike.Code.Player;

/// <summary>
/// Pure C# inventory logic for storing and using items.
/// Follows the decoupling pattern: emits events for UI updates.
/// </summary>
public class Inventory
{
    private readonly List<IItem> _items = new();
    public IReadOnlyList<IItem> Items => _items;
    public int MaxSlots { get; }
    public int Count => _items.Count;
    public bool IsFull => _items.Count >= MaxSlots;

    public event Action OnInventoryChanged;

    public Inventory(int maxSlots = 10)
    {
        MaxSlots = maxSlots;
    }

    /// <summary>
    /// Attempts to add an item to the inventory.
    /// Returns true if successful, false if inventory is full.
    /// </summary>
    public bool AddItem(IItem item)
    {
        if (IsFull)
            return false;

        _items.Add(item);
        OnInventoryChanged?.Invoke();
        return true;
    }

    /// <summary>
    /// Uses the item at the given slot index.
    /// Returns true if the item was successfully used.
    /// </summary>
    public bool UseItem(int index, Entities.IActor user)
    {
        if (index < 0 || index >= _items.Count)
            return false;

        var item = _items[index];
        bool wasUsed = item.Use(user);

        if (wasUsed && item.IsConsumable)
        {
            _items.RemoveAt(index);
            OnInventoryChanged?.Invoke();
        }

        return wasUsed;
    }

    /// <summary>
    /// Gets the item at the given slot index, or null if empty.
    /// </summary>
    public IItem GetItem(int index)
    {
        if (index < 0 || index >= _items.Count)
            return null;
        return _items[index];
    }

    /// <summary>
    /// Clears all items from the inventory.
    /// </summary>
    public void Clear()
    {
        _items.Clear();
        OnInventoryChanged?.Invoke();
    }
}
