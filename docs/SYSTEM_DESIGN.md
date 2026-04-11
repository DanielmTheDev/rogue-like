# RogueLike Architecture Ledger

## Dependencies & Core Systems

- **DungeonGrid:** Pure C# data structure. Maps grid indices to Walkability and tracks out-of-bounds.
- **GridMover:** Pure C# movement logic. Validates pathing via `DungeonGrid`.
- **TurnManager:** Pure C# state machine. Enforces sequential game loop (Player Action -> Enemy Action -> Repeat).
- **PlayerController:** Godot `Node2D`. Handles Input -> translates to `GridMover` calls. Submits actions to `TurnManager` to consume turns.
- **DungeonTileMap:** Godot `TileMapLayer`. Listens to initial structure to draw checkerboard tiles.
