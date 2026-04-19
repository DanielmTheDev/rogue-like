# Cleanup: Remove Test Spawns & Update Agent Rules

## High-Level Outline
1. Remove temporary test Goblin/Potion spawning from Main.cs
2. Keep random enemy spawning active
3. Add random potion spawning to rooms (1-2 potions per dungeon)
4. Update AGENTS.md with refactoring guidance

## Reasoning
Test spawns were useful for Phase 2 development. Now we restore normal gameplay loop.

## Architecture Impact
None. Just cleanup.

- [ ] Remove test spawns from Main.cs
- [ ] Add potion spawning to Spawner
- [ ] Remove debug spawn logging
- [ ] Update AGENTS.md
