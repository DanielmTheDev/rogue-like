# RogueLike Architecture Ledger

## Dependencies & Core Systems

- **Map Generators:** Procedural generation MUST NEVER reside in `DungeonGrid`. Complex mapping logic must be extracted into static or standalone builder classes (e.g., `BspDungeonGenerator`) that operate on a pure `DungeonGrid`.
- **DungeonGrid:** Pure C# data structure. Maps grid indices to Walkability and tracks out-of-bounds. Default constructor yields a blank featureless floor.
- **EntityManager/IActor:** The core dynamic Entity registry. `GridMover` coordinates with `EntityManager` to ensure no two `IActor` instances overlap.
- **CombatSystem & HealthController:** Pure C# logic. `ICombatant` extends `IActor` to carry `HealthController`. `CombatSystem.ResolveBump()` performs interactions outside of standard `GridMover` logic.
- **GridMover:** Pure C# movement logic. Validates pathing via `DungeonGrid` and `EntityManager`. Supports 8-directional movement; rejects diagonal corner-cutting via `DungeonGrid.IsDiagonalCornerCut`.
- **DungeonGrid traversability:** The no-corner-cut rule lives on `DungeonGrid` (`IsDiagonalCornerCut`, next to `IsWalkable`) — it is a query over grid topology, so the grid owns it. Both `GridMover` (actual moves) and `Pathfinder` (planned paths) call it, so plans never include steps the mover would reject.
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
- **Level Settings:** Godot `Resource` based configuration (`LevelSettings.cs`). Allows tuning spawning parameters (density, difficulty scaling) directly from the Inspector.
- **ItemManager:** Pure C# logic for tracking floor items and managing pickups.
- **Inventory:** Pure C# component owned by PlayerController. Stores up to 10 items. Emits events for UI updates.
- **ExperienceSystem:** Pure C# logic for XP, leveling, and stat progression. Decoupled from UI via events.
- **Level Generation:** `Main.cs` orchestrates level creation, cleanup, and populating new dungeons when the player uses the stairs. Player state is preserved across levels.
- **Pathfinding:** A pure C# A* `Pathfinder` class provides navigation for AI. Enemies use this to pursue the player when they are visible within the FOV. 8-directional: `GetNeighbors` emits cardinal + diagonal neighbors (skipping corner-cuts via `MovementRules`), and `GetDistance` uses octile distance (D=10 cardinal, D2=14 diagonal) to keep the heuristic admissible. `EnemyAI` derives its step direction from the path and so moves diagonally with no further changes.
- **Game State:** `Main.cs` also manages the game state, handling level transitions and the restart-on-death flow.
- **Advanced Player Actions:**
  - **Shift Move:** Hold Shift + Direction to auto-move until seeing an enemy, hitting a wall, or encountering a corner.
  - **Wait Full Health:** Press Shift + `.` to rest repeatedly until health is fully restored.
    - **Safety:** The action stops automatically if an enemy is spotted, if the player takes damage, or after 200 turns.
  - **Passive Healing:** Player now heals 1 HP every 5 turns automatically.
  - Both actions consume multiple turns and integrate with FOV system for enemy detection.
- **DungeonTileMap & FovTileMap:** Godot `TileMapLayer` nodes. They listen to the purely logical data grids to render specific visual sprites.
- **Minimap:** Godot `Control` node (`MinimapController.cs`) that renders a compact 100×100px grid overview in the bottom-right corner. Data sources: `DungeonGrid` (cell types) and `FovMap` (visibility states). Coloring: Unexplored=hidden, Explored=dark gray, Visible=light gray, walls slightly darker, player=yellow dot. Updated via `Refresh()` which triggers `QueueRedraw()`, called from `Main.UpdateFov()` after each `ComputeFov()`.

## Domain Layer (target architecture)

The codebase is migrating toward a **rich domain model** under full DDD. Target layering (hard physical split):

- `Code/Domain/` — pure C#, **never `using Godot;`**. Value Objects, aggregate roots, world/turn logic, domain events.
- `Code/View/` — Godot nodes only. Render + input, no game rules. A single `GodotConv` bridge converts `Vector2I`↔`GridPos`/`Direction`.

**Value Object catalogue** (immutable `readonly record struct`, invariants in ctor, equality-by-value, no setters):

| VO | Invariant | Replaces |
|----|-----------|----------|
| `GridPos(X,Y)` | none (bounds are the grid's job) | `Vector2I` for positions in the domain |
| `Direction(Dx,Dy)` | Dx,Dy ∈ {-1,0,1} | raw `Vector2I` direction deltas |
| `Health(Current,Max)` | Max>0, 0≤Current≤Max | mutable `HealthController` state |
| `Damage(Amount)` | Amount≥0 | `int AttackDamage` |
| `XpAmount(Value)` (optional) | Value≥0 | `int` XP |

**Aggregate roots** (pure C#, own state + behavior, raise domain events, no public setters): `Actor` (→ `TryMove`, `Attack`, `TakeDamage`), `Player` / `Enemy` / `Archer` (own their turn/decision behavior), plus `Dungeon`, `ActorRegistry`, `Inventory`, `FloorItems`, `ExperienceTrack`, `TurnEngine`. Godot controllers become Views that observe domain events and render.

### Migration status (anemic → rich)

| System | Status | Notes |
|--------|--------|-------|
| HealthController | 🟡 rich-mutable | clamp/invariants present; to become `Health` VO + events on `Actor` |
| DungeonGrid | 🟢 rich | pixel math to move to `GodotConv`; `Vector2I`→`GridPos` pending |
| GridMover | 🟢 rich | to be absorbed into `Actor.TryMove` |
| Inventory | 🟢 rich | keep |
| ExperienceSystem | 🟢 rich | → `ExperienceTrack`, `int`→`XpAmount` |
| FovMap / Raycaster | 🟢 rich | `Vector2I`→`GridPos` pending |
| TurnManager | 🟢 rich | → `TurnEngine`, absorb enemy-phase loop |
| Pathfinder / LineOfSight | 🟢 pure-util | `Vector2I`→`GridPos`, `Mathf`→`Math` |
| **CombatSystem** | 🔴 anemic | static mutator → behavior onto the combatant; to be deleted |
| **EnemyAI / ArcherAI** | 🔴 anemic | behavior split from entity → absorb into `Enemy`/`Archer` |
| **ItemManager** | 🔴 anemic | pickup orchestration → `Item.TryPickup`; → `FloorItems` |
| **Controllers (Player/Enemy/Archer/Actor)** | 🔴 are-the-entity | implement `IActor`/`ICombatant` today → demote to Views |

Update this table as each migration batch lands. 🔴 = anemic/anti-pattern present, 🟡 = partially rich, 🟢 = rich/clean.
