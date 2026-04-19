# Refactoring: Compact Inventory UI with Item Stacking

## High-Level Outline
1. Make InventoryUI window smaller (half the height)
2. Change display logic to group same items with count
3. Update input handling to work with grouped slots

## Example
**Old Display:**
```
1. Healing Potion
2. Healing Potion
3. <empty>
```

**New Display:**
```
1. Healing Potion x2
2. <empty>
```

## Architecture Impact
- `Inventory.cs`: Add `GetGroupedItems()` method
- `InventoryUI.cs`: Update display logic
- `PlayerController.cs`: Map key presses to grouped slots
- Internal storage stays as `List<IItem>` (no change)

- [ ] Add Inventory.GetGroupedItems()
- [ ] Update InventoryUI display logic
- [ ] Update PlayerController input handling
- [ ] Shrink InventoryUI.tscn window
