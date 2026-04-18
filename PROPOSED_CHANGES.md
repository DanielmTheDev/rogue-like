# Refactoring: HealingPotion Scene + Remove Debug Heal

## High-Level Outline
1. Remove debug heal (Health.Heal(1)) from PlayerController movement.
2. Create proper HealingPotion.tscn with Sprite2D child.
3. Update HealingPotion.cs to remove sprite creation code.
4. Update Spawner to load PackedScene instead of direct instantiation.

## Reasoning
- **Scene Consistency:** All entities (Player, Goblin, Archer) use .tscn. Items should too.
- **Gameplay Balance:** Debug heal makes game too easy for real testing.
- **Maintainability:** Sprite should be editable in Inspector, not hardcoded.

## Architecture Impact
None. Pure cleanup/consistency improvements.

- [ ] Remove debug heal from PlayerController.cs
- [ ] Create proper Scenes/HealingPotion.tscn
- [ ] Clean up HealingPotion.cs
- [ ] Update Spawner.cs to use PackedScene
