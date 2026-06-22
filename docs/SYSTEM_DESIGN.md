# RogueLike Architecture Ledger

## Dependencies & Core Systems

- **Map Generators:** Procedural generation MUST NEVER reside in `DungeonGrid`. Complex mapping logic must be extracted into static or standalone builder classes (e.g., `BspDungeonGenerator`) that operate on a pure `DungeonGrid`.
- **DungeonGrid:** Pure C# data structure. Maps grid indices to Walkability and tracks out-of-bounds. Default constructor yields a blank featureless floor.
- **EntityManager/IActor:** The core dynamic Entity registry. `GridMover` coordinates with `EntityManager` to ensure no two `IActor` instances overlap.
- **Combat & HealthController:** Pure C# logic. `ICombatant` extends `IActor` to carry `HealthController`. Attack behavior lives on the combatant itself: `ICombatant.TryAttack(defender)` (default interface method) deals damage + logs, and invokes the `OnKilled(victim)` kill-reaction hook on death (the player overrides it to gain XP). The old static `CombatSystem` service has been removed (rich-domain migration); callers invoke `attacker.TryAttack(defender)` directly.
- **GridMover:** Pure C# movement logic. Supports 8-directional movement. Validates each step via `DungeonGrid.CanStep` (the shared terrain rule), then adds its own occupancy check via `EntityManager`.
- **DungeonGrid traversability:** The terrain rule lives on `DungeonGrid` as `CanStep(GridPos, Direction)` = walkable target **and** not a diagonal corner-cut (built on `IsWalkable` + `IsDiagonalCornerCut`). It is a query over grid topology, so the grid owns it. Reused by both `GridMover` (actual moves) and pathfinding (planned paths), so plans never include steps the mover would reject. Occupancy is **not** part of `CanStep` — that's the mover's concern (the grid knows walls, not actors).
- **Where spatial logic lives (decided principle):** single-impl topology queries that read the grid's own cells fold **onto** `DungeonGrid` (line-of-sight, pathfinding) — the grid owns the walls, so the query lives with the data. Pluggable/swappable algorithms stay **separate injected interfaces** (FOV keeps `IFovAlgorithm`, selected in `Main`). This is the line that keeps the grid cohesive rather than a dumping ground.
- **FOV Array:** Pure C# data structures isolating visibility calculations.
    - `FovMap`: Pure struct tracking `VisibilityState` of every cell.
    - `IFovAlgorithm`: Interface for algorithms (`Raycaster`) that mutate the `FovMap`.
- **EnemyAI:** Pure C# decision tree. Finds the player via `EntityManager` and paths towards them. Used by melee Goblins.
- **ArcherAI:** Pure C# decision tree for ranged enemies. Calls `DungeonGrid.HasClearLine` to check for clear shots and `GridPos.ManhattanTo` for range. Behavior: shoot if in LOS + range, chase if in LOS but out of range, idle otherwise.
- **Line-of-sight:** A query on `DungeonGrid` (`HasClearLine(GridPos, GridPos)`) — the grid owns the walls, so the LOS query lives with that data. Traces with Bresenham over the grid's own cells; endpoints excluded from the wall check.
- **EnemyController / ArcherController / PlayerController:** Godot `Node2D` classes extending `ActorController`. They delegate logic downward to `GridMover`/`EnemyAI`/`ArcherAI` and only handle visual synchronization and input.
- **ActorController:** Abstract base class for all grid actors. Centralizes `HealthController`, `[Export]` variables (`BaseHealth`, `BaseAttackDamage`, `HealthBar`), and `Die()` logic.
- **Spawner:** Static utility handling entity instantiation and placement. Supports multiple enemy types (Goblins and Archers) and alternates them across BSP rooms.
- **TurnManager:** Pure C# state machine. Enforces sequential game loop (Player Action -> Enemy Action -> Repeat).
- **GameLog:** Pure C# service managing message history. Decoupled from UI via events.
- **Level Settings:** Godot `Resource` based configuration (`LevelSettings.cs`). Allows tuning spawning parameters (density, difficulty scaling) directly from the Inspector.
- **FloorItems:** Pure C# (Godot-free) registry for tracking floor items and managing pickups. Lives in `Code/Domain/Items/`; `GridPos`-native (was `ItemManager`/`Vector2I`).
- **Inventory:** Pure C# component owned by PlayerController. Stores up to 10 items. Emits events for UI updates.
- **ExperienceSystem:** Pure C# logic for XP, leveling, and stat progression. Decoupled from UI via events.
- **Level Generation:** `Main.cs` orchestrates level creation, cleanup, and populating new dungeons when the player uses the stairs. Player state is preserved across levels.
- **Pathfinding:** A* navigation is a query over the grid's own cells, so it lives **on `DungeonGrid`** (`FindPath`, folded in from the deleted `Pathfinder` class, 2.3b). The only public surface is `grid.FindPath(GridPos, GridPos)` (`GridPos`-only since 2.4); the A* work runs in a **hidden per-search `PathSearch` object** so its mutable open/closed state never lives on the long-lived grid. 8-directional: neighbours come from `grid.WalkableNeighbors` (`Direction.AllEight` filtered by `CanStep`), and an octile heuristic (D=10 cardinal, D2=14 diagonal) keeps it admissible. Enemies (`EnemyAI`/`ArcherAI`) call `grid.FindPath` to pursue a visible player and derive their step direction from the path.
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

- `Code/Domain/` — pure C#, **never `using Godot;`** — **enforced**: an MSBuild guard (`GuardDomainGodotFree` in `RogueLike.csproj`) fails the build on any `^using Godot` under `Code/Domain/`. Value Objects, aggregate roots, world/turn logic, domain events.
- `Code/View/` — Godot nodes only. Render + input, no game rules. The `GridConversions` extension class is the single bridge: `Vector2I`↔`GridPos` plus pixel↔grid (`ToWorldCenter`/`ToGridPos(world, tileSize)`). Extension methods so conversions read off the value (`coord.ToGridPos()`); domain stays Godot-free.

**Layer move — Phase 1 landed (DDD 2.6):** `Code/Domain/` now physically holds the Godot-free slices: `Common/` (GridPos, Direction), `Combat/` (Damage, HealthController, ICombatant), `Actors/` (IActor, EnemyAI, ArcherAI), `Items/` (Inventory), `Flow/` (TurnManager, TurnState, ExperienceSystem, GameLog), `Grid/FOV/` (FovMap, Raycaster, IFovAlgorithm, VisibilityState). The guard is active. **Not yet moved (graduate when their last Godot bit goes):** `DungeonGrid` (+CellType, generators — partial class, `Vector2I Size`), `GridMover` (`Vector2 WorldPosition`), `EntityManager` (`Node2D` map), `ItemManager`/`IItem` (`Vector2I`). **Phase 2 landed (DDD 2.6):** the Godot view nodes moved into `Code/View/` (mirrored subfolders, namespaces `RogueLike.Code.View.*`): `Main`, `Player/` (PlayerController, InputMapper), `Enemies/` (Enemy/ArcherController), `Entities/` (ActorController, Spawner), `Grid/` (DungeonTileMap) + `Grid/FOV/` (FovTileMap), `Items/` (ItemController) + `Items/Consumables/` (HealingPotion), `UI/` (Experience/GameLog/Minimap/Inventory), `World/` (StairsController), `Resources/` (LevelSettings), alongside the existing `GridConversions`. The 12 `res://` script paths (11 `.tscn` + `DefaultLevelSettings.tres`) were repointed; `.cs.uid` siblings moved with each file so scene `uid://` refs stay valid. **Stayed behind (transitional, graduate to `Domain/` when their last Godot bit goes):** `DungeonGrid` (+CellType, generators), `GridMover`, `EntityManager` — still `using Godot;` (`Vector2I`/`Rect2I`/`Node2D`) but they're domain logic, not Godot nodes; `EntityManager` (→ `ActorRegistry`) is consumed by `Domain/Actors/` AIs, so it must stay reachable from the Godot-free layer. (`Flow/` = the realized name for the turn/progression/log slice; FOV sits under `Domain/Grid/` since the `Dungeon` aggregate hasn't graduated yet.) **Straddler graduation #1 landed (DDD 2.6):** `IItem`/`ItemManager` lost their last Godot bit (`Vector2I`→`GridPos`), so both moved into `Code/Domain/Items/` (`ItemManager`→`FloorItems`); 3 straddlers remain (`DungeonGrid`, `GridMover`, `EntityManager`).

**Domain is organized as vertical slices, not by technical kind.** Each slice owns its types: `Domain/Combat/` (Damage, Health, attack/kill), `Domain/Actors/`, `Domain/Items/`, `Domain/Progression/`, `Domain/World/` (Dungeon — which owns the line-of-sight **and** pathfinding queries over its own cells — and FOV). Pathfinding is **not** a separate type: it folds onto the Dungeon (see "Where spatial logic lives"). `Domain/Common/` is the **thin shared kernel** — ONLY cross-cutting VOs used by many slices (`GridPos`, `Direction`). A type goes in `Common/` only if multiple slices need it; otherwise it lives in its owning slice.

**Value Object catalogue** (immutable `readonly record struct`, invariants in ctor, equality-by-value, no setters):

| VO | Slice | Invariant | Replaces |
|----|-------|-----------|----------|
| `GridPos(X,Y)` ✅ landed (2.2) | Common | none (bounds are the grid's job) | `Vector2I` for positions in the domain |
| `Direction(Dx,Dy)` ✅ landed (2.2) | Common | Dx,Dy ∈ {-1,0,1} | raw `Vector2I` direction deltas |
| `Health(Current,Max)` | Combat | Max>0, 0≤Current≤Max | mutable `HealthController` state |
| `Damage(Amount)` ✅ landed (2.1) | Combat | Amount≥0 | `int AttackDamage` |
| `XpAmount(Value)` (optional) | Progression | Value≥0 | `int` XP |

`Damage` lives at `Code/Domain/Combat/` (`RogueLike.Code.Domain.Combat`) and is constructed inside `ICombatant.TryAttack` (unwrapped to `int` for `HealthController` until the `Health` VO lands in 2.5).

**Aggregate roots** (pure C#, own state + behavior, raise domain events, no public setters): `Actor` (→ `TryMove`, `Attack`, `TakeDamage`), `Player` / `Enemy` / `Archer` (own their turn/decision behavior), plus `Dungeon`, `ActorRegistry`, `Inventory`, `FloorItems`, `ExperienceTrack`, `TurnEngine`. Godot controllers become Views that observe domain events and render.

### Migration status (anemic → rich)

| System | Status | Notes |
|--------|--------|-------|
| HealthController | 🟡 rich-mutable | clamp/invariants present; to become `Health` VO + events on `Actor` |
| DungeonGrid | 🟢 rich | ✅ (2.3a) owns `HasClearLine`; ✅ (2.3b) owns pathfinding — `FindPath`/`CanStep`/`WalkableNeighbors` + hidden `PathSearch`. ✅ (2.3c) cell API (`IsInBounds`/`IsWalkable`/`GetCell`/`SetCell`) + interior now `GridPos`-native; `IsDiagonalCornerCut` private (`GridPos`/`Direction`); pixel↔grid math **removed** from the grid → lives on the view side as `GridConversions` extensions. ✅ (2.4) `FindPath` is `GridPos`-only (the `Vector2I` edge deleted); actor/movement callers flipped to `GridPos`. Thin `Vector2I` cell overloads (`IsInBounds`/`IsWalkable`/`GetCell`/`SetCell`) **remain** `// TRANSITIONAL` for the still-`Vector2I` callers (map generators + view renderers + their tests) — retire when those migrate; `Size` stays `Vector2I` (dimension, not a position) |
| GridMover | 🟢 rich | ✅ (2.3b) validates via `DungeonGrid.CanStep`; ✅ (2.4) position is `GridPos` and `TryMove(Direction)` takes the `Direction` VO (`_gridPosition.Step(direction)`) — no `Vector2I` left. To be absorbed into `Actor.TryMove` |
| Inventory | 🟢 rich | keep |
| ExperienceSystem | 🟢 rich | → `ExperienceTrack`, `int`→`XpAmount` |
| FovMap / Raycaster | 🟢 rich | ✅ (2.3d) `GridPos`-native + **Godot-free** (`IFovAlgorithm`/`FovMap`/`Raycaster` dropped `using Godot;`); view-side callers convert via `GridConversions.ToGridPos` at the edge. `grid.Size.X/Y` ints kept inline (Size stays `Vector2I`) |
| TurnManager | 🟢 rich | → `TurnEngine`, absorb enemy-phase loop |
| ~~LineOfSight~~ | 🟢 done | ✅ (2.3a) deleted; `HasClearLine` folded onto `DungeonGrid` as a GridPos query (grid owns the walls), `ManhattanDistance` dropped → `GridPos.ManhattanTo` |
| ~~Pathfinder~~ | 🟢 done | ✅ (2.3b) class **deleted**; A* folded onto `DungeonGrid` (hidden `PathSearch`, `GridPos`-native) since pathfinding is a query over the grid's cells. All `new Pathfinder()` DI threading (Main→Spawner→Controllers→AI) removed; AIs call `grid.FindPath`. `WalkableNeighbors`+`CanStep` reuse the grid's walls directly (no `Vector2I` bridge). ✅ (2.4) the `Vector2I` `FindPath` edge **deleted** — `FindPath` is `GridPos`-only and the file is now Godot-free |
| ~~CombatSystem~~ | 🟢 done | deleted; attack behavior lives on `ICombatant.TryAttack` + `OnKilled` hook. Callers (`PlayerController`, `EnemyAI`, `ArcherAI`) invoke `attacker.TryAttack(defender)` directly |
| **EnemyAI / ArcherAI** | 🟡 decision-tree | no longer reference the deleted `CombatSystem`; drive the entity's own verbs (`combatant.TryAttack`, `mover.TryMove`). ✅ (2.4) `GridPos`-native + **Godot-free** (derive their step via `GridPosition.DirectionTo(path[0])`). Test coverage locks the attack paths (enemy bump, archer ranged shot). Still a separate object from the entity — full absorption into `Enemy`/`Archer` aggregates is Phase 3.3 |
| **FloorItems** / **IItem** | 🟢 graduated | ✅ (DDD 2.6 straddler #1) `Vector2I`→`GridPos` (`IItem.GridPosition`, `FloorItems.CheckForPickup`); both dropped `using Godot;` and **moved to `Code/Domain/Items/`** alongside `Inventory`; `ItemManager`→`FloorItems`. The `PlayerController` `.ToVector2I()` pickup bridge deleted. Pickup decision lives on `IItem.TryPickup` (default interface method); node self-frees via `ItemController.OnPickup`; `FloorItems` only finds + delegates + unregisters. **Phase-3 target: pickup ownership flips to `Player.TryPickup(item)`** — the actor aggregate owns the acquire + its own inventory; the item keeps only `CanPickup`/`OnPickup`. `item.TryPickup(actor, inventory)` is a transitional placement (an item mutating another aggregate's inventory); the verb belongs to the actor that owns the inventory boundary. |
| **Controllers (Player/Enemy/Archer/Actor)** | 🔴 are-the-entity | implement `IActor`/`ICombatant` today → demote to Views |

Update this table as each migration batch lands. 🔴 = anemic/anti-pattern present, 🟡 = partially rich, 🟢 = rich/clean.
