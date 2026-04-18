# Proposed Changes: Phase 2 - The Backpack

## High-Level Outline
1. Create `Code/Player/Inventory.cs` (pure C# logic)
2. Create `tests/Player/InventoryTest.cs` (unit tests)
3. Refactor `IItem` interface to support pickup vs. use
4. Update `HealingPotion` to implement new methods
5. Wire `PlayerController` to use inventory
6. Update `ItemManager.CheckForPickup` to add to inventory
7. Create `InventoryUI.cs` and `InventoryUI.tscn`
8. Add UI to Main scene
9. Update docs/SYSTEM_DESIGN.md

## Reasoning
Pure C# `Inventory` class is testable and follows established patterns. Auto-pickup keeps UX simple.

## Architecture Impact
- New component: `Inventory` (owned by PlayerController)
- IItem gets `CanPickup()`, `Use()`, `OnPickup()` methods
- Items now have two lifecycles: Floor → Inventory → Use

- [ ] Create Inventory.cs
- [ ] Create InventoryTest.cs
- [ ] Refactor IItem interface
- [ ] Update HealingPotion
- [ ] Wire PlayerController
- [ ] Update ItemManager
- [ ] Create InventoryUI
- [ ] Update docs/SYSTEM_DESIGN.md
