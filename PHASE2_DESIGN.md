# Phase 2: The Backpack - Architecture Design

## Core Concept
Instead of items being consumed instantly when walked over, the player can now:
1. **Pick up** items (they go into a bag)
2. **Store** multiple items
3. **Use** items via hotkeys (1-9)

---

## Architecture Components

### 1. Inventory (Pure C# Logic)
```csharp
public class Inventory {
    private List<IItem> _items = new();
    public IReadOnlyList<IItem> Items => _items;
    public int MaxSlots { get; } = 10;
    
    public event Action OnInventoryChanged;
    
    public bool AddItem(IItem item);      // Returns false if full
    public void UseItem(int index, IActor user);
    public void DropItem(int index);
}
```

**Why Pure C#?** 
- Testable (we can write unit tests).
- Follows the pattern established by `HealthController`, `GridMover`.

---

### 2. IItem Refactor: Pickup vs. Use
**Current:** `ProcessPickup(actor)` is called immediately when walked over.
**Phase 2:** Split into two methods:

```csharp
public interface IItem {
    bool CanPickup(IActor actor);     // "Can I carry this?"
    void OnPickup(IActor actor);      // "I was picked up" (for logging)
    bool Use(IActor actor);            // "Use me from inventory"
    bool IsConsumable { get; }         // Should item disappear after use?
}
```

**Example Flow:**
- Player walks over Potion
- `PlayerController` checks `Inventory.AddItem(potion)`
- If space: Item calls `OnPickup()`, gets added to bag
- If full: Log "Inventory full!"

---

### 3. Input Handling (PlayerController)
Add new input actions:
- **G** (Get): Explicitly pick up item at current position
- **1-9**: Use item from slot
- **D** (Drop): Drop item from inventory back to floor

```csharp
if (keyEvent.Keycode == Key.Key1) {
    _inventory.UseItem(0, this);
    _turnManager.EndPlayerTurn();
}
```

---

### 4. UI Display (InventoryUI.cs)
A simple text panel showing:
```
Inventory (3/10)
1. Healing Potion
2. Healing Potion
3. <empty>
```

**Architecture:**
- `InventoryUI` is a `Control` node in `CanvasLayer`
- Subscribes to `Inventory.OnInventoryChanged`
- Updates a `RichTextLabel` with the current list

---

## Integration Points

### PlayerController Changes:
```csharp
private Inventory _inventory;

public void Initialize(..., ItemManager itemManager, ...) {
    _inventory = new Inventory();
    _inventory.OnInventoryChanged += UpdateInventoryUI;
}

// In TryMove():
_itemManager?.CheckForPickup(GridPosition, this, _inventory);
```

### ItemManager Changes:
```csharp
public void CheckForPickup(Vector2I position, IActor actor, Inventory inventory) {
    var item = GetItemAt(position);
    if (item != null && item.CanPickup(actor)) {
        if (inventory.AddItem(item)) {
            item.OnPickup(actor);
            UnregisterItem(item);
        } else {
            GameLog.Instance.Log("Inventory full!");
        }
    }
}
```

---

## Incremental Implementation Steps

### Step 1: Core Inventory Logic
- Create `Code/Player/Inventory.cs`
- Add `AddItem`, `UseItem` methods
- Write unit tests

### Step 2: Refactor IItem
- Add `Use()` and `CanPickup()` to interface
- Update `HealingPotion` to implement new methods
- Keep `ProcessPickup()` as a fallback for now

### Step 3: Wire PlayerController
- Add `_inventory` field
- Add input handling for 1-9 keys
- Connect to `ItemManager`

### Step 4: Add Simple UI
- Create `InventoryUI.cs` and `Scenes/InventoryUI.tscn`
- Display as text list (no fancy sprites yet)
- Add to `Main.tscn`

---

## What Stays the Same
- `ItemManager` still tracks floor items
- `DungeonGrid` unchanged
- `TurnManager` unchanged
- We keep the test Goblin/Potion spawning for now

---

## Questions Before We Start?
1. Should we add a "full inventory" sound/effect?
2. Should pressing G be required, or auto-pickup on walk?
3. Should we add a "Drop" feature in Phase 2, or wait for Phase 3?

**My Recommendation:** 
- Auto-pickup on walk (simpler UX)
- No drop yet (Phase 3)
- Just log messages for now (no sounds)

Sound good?
