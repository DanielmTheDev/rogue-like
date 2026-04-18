# RogueLike Architecture Ledger

## Dependencies & Core Systems

- **Map Generators:** Procedural generation MUST NEVER reside in `DungeonGrid`. Complex mapping logic must be extracted into static or standalone builder classes (e.g., `BspDungeonGenerator`) that operate on a pure `DungeonGrid`.
- **DungeonGrid:** Pure C# data structure. Maps grid indices to Walkability and tracks out-of-bounds. Default constructor yields a blank featureless floor.
- **EntityManager/IActor:** The core dynamic Entity registry. `GridMover` coordinates with `EntityManager` to ensure no two `IActor` instances overlap.
- **CombatSystem & HealthController:** Pure C# logic. `ICombatant` extends `IActor` to carry `HealthController`. `CombatSystem.ResolveBump()` performs interactions outside of standard `GridMover` logic.
- **GridMover:** Pure C# movement logic. Validates pathing via `DungeonGrid` and `EntityManager`.
- **FOV Array:** Pure C# data structures isolating visibility calculations.
    - `FovMap`: Pure struct tracking `VisibilityState` of every cell.
    - `IFovAlgorithm`: Interface for algorithms (`Raycaster`) that mutate the `FovMap`.
- **EnemyAI:** Pure C# decision tree. Finds the player via `EntityManager` and paths towards them. Used by melee Goblins.
- **ArcherAI:** Pure C# decision tree for ranged enemies. Uses `LineOfSight` to check for clear shots. Behavior: shoot if in LOS + range, chase if in LOS but out of range, idle otherwise.
- **LineOfSight:** Pure C# utility using Bresenham's line algorithm. Answers "is there an unobstructed path between tile A and tile B?" Shared by `ArcherAI` and potentially future systems.
- **EnemyController / ArcherController / PlayerController:** Godot `Node2D` classes extending `ActorController`. They delegate logic downward to `GridMover`/`EnemyAI`/`ArcherAI` and only handle visual synchronization and input.
- **ActorController:** Abstract base class for all grid actors. Centralizes `HealthController`, `[Export]` variables (`BaseHealth`, `BaseAttackDamage`, `HealthBar`), and `Die()` logic.
- **Spawner:** Static utility handling entity instantiation and placement. Supports multiple enemy types (Goblins and Archers) and alternates them across BSP rooms.
- **TurnManager:** Pure C# state machine. Enforces sequential game loop (Player Action -> Enemy Action -> Repeat).
- **GameLog:** Pure C# service managing message history. Decoupled from UI via events.
- **ItemManager:** Pure C# logic for tracking floor items and managing pickups.
- **Inventory:** Pure C# component owned by PlayerController. Stores up to 10 items. Emits events for UI updates.
- **DungeonTileMap & FovTileMap:** Godot `TileMapLayer` nodes. They listen to the purely logical data grids to render specific visual sprites.
