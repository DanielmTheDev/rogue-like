# RogueLike Architecture Ledger

## Dependencies & Core Systems

- **Map Generators:** Procedural generation MUST NEVER reside in `DungeonGrid`. Complex mapping logic must be extracted into static or standalone builder classes (e.g., `PillarArenaGenerator`) that operate on a pure `DungeonGrid`.
- **DungeonGrid:** Pure C# data structure. Maps grid indices to Walkability and tracks out-of-bounds. Default constructor yields a blank featureless floor.
- **GridMover:** Pure C# movement logic. Validates pathing via `DungeonGrid`.
- **TurnManager:** Pure C# state machine. Enforces sequential game loop (Player Action -> Enemy Action -> Repeat).
- **PlayerController:** Godot `Node2D`. Handles Input -> translates to `GridMover` calls. Submits actions to `TurnManager` to consume turns.
- **DungeonTileMap:** Godot `TileMapLayer`. Listens to initial structure to draw checkerboard tiles.
