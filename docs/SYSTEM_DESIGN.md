# RogueLike Architecture Ledger

## Dependencies & Core Systems

- **Map Generators:** Procedural generation MUST NEVER reside in `DungeonGrid`. Complex mapping logic must be extracted into static or standalone builder classes (e.g., `PillarArenaGenerator`) that operate on a pure `DungeonGrid`.
- **DungeonGrid:** Pure C# data structure. Maps grid indices to Walkability and tracks out-of-bounds. Default constructor yields a blank featureless floor.
- **EntityManager/IActor:** The core dynamic Entity registry. `GridMover` coordinates with `EntityManager` to ensure no two `IActor` instances overlap.
- **GridMover:** Pure C# movement logic. Validates pathing via `DungeonGrid` and `EntityManager`.
- **EnemyAI:** Pure C# decision tree. Finds the player via `EntityManager` and paths towards them.
- **EnemyController / PlayerController:** Godot `Node2D` classes mapped to `IActor`. They delegate logic downward to `GridMover`/`EnemyAI` and only handle visual synchronization and input.
- **TurnManager:** Pure C# state machine. Enforces sequential game loop (Player Action -> Enemy Action -> Repeat).
- **DungeonTileMap:** Godot `TileMapLayer`. Listens to initial structure to draw checkerboard tiles.
