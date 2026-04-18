# Proposed Changes: Iterative Inventory Phase 1 (Instant Consumables)

## High-Level Outline
1. **Interface:** Create `Code/Items/IItem.cs` to define basic floor items.
2. **Actor:** Create `Code/Items/ItemController.cs` (Node2D) for visual floor items.
3. **Logic:** Create `Code/Items/ItemManager.cs` to track all items on the grid.
4. **Integration:** Update `Code/Grid/GridMover.cs` or `PlayerController.cs` to trigger `ItemManager.CheckForPickup(position)` after movement.
5. **Assets:** Use Nano Banana Pro (`image_gen`) to create a 32x32 Healing Potion.

## Reasoning
This fits our architecture by mirroring the `EntityManager` pattern. Keeping items in their own manager ensures the `DungeonGrid` stays as a "pure data" floor map and doesn't get cluttered with entity/item logic.

## Architecture Impact
- **New Directory:** `Code/Items/`.
- **New Manager:** `ItemManager` will be held by `Main.cs`.
- **Decoupling:** Items are separate from Actors. Goblins won't pick them up (yet).

- [ ] Create Code/Items/IItem.cs
- [ ] Create Code/Items/ItemController.cs
- [ ] Create Code/Items/ItemManager.cs
- [ ] Generate Potion Sprite (Nano Banana Pro)
- [ ] Integrate into Main.cs & PlayerController.cs
- [ ] Update docs/SYSTEM_DESIGN.md
