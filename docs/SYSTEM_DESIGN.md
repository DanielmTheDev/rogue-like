# System Design — RogueLike

> Architecture ledger. Consult this before every task. Update when systems change.

---

## Overview

A DCSS-inspired top-down, turn-based roguelike built in Godot 4.5 with C#.
Core design principle: **data-driven logic separated from Godot nodes** for testability.

---

## Systems

### 1. Grid System

**Purpose:** Represents the dungeon as a 2D tile grid.

| Component | Type | Responsibility |
|-----------|------|---------------|
| `GridMap` | Pure C# class | Data model: bounds, walkability, coordinate conversion |
| `CellType` | Enum | `Floor`, `Wall` |
| `DungeonTileMap` | `TileMapLayer` (Godot) | Visual rendering of GridMap as a checkerboard |

**Key decisions:**
- `GridMap` has **no Godot Node dependency** — it uses `Vector2I`/`Vector2` (Godot structs) but doesn't extend `Node` or `Resource`. This allows fast unit testing.
- Out-of-bounds coordinates are treated as `Wall` (unwalkable).
- Tile size: **32px**. Grid default: **20×15 tiles**.
- Checkerboard uses two atlas tiles (light/dark stone).

**Communication pattern:**
- `Main.cs` creates `GridMap` and passes it to both `DungeonTileMap.Render()` and `PlayerController.Initialize()` (downward injection).

---

### 2. Player System

**Purpose:** Handles player input and grid-based movement.

| Component | Type | Responsibility |
|-----------|------|---------------|
| `PlayerController` | `Node2D` (Godot) | Input handling, grid movement, position snapping |

**Key decisions:**
- Movement is **instant snap** (no tweening) — keeps turn-based feel clean.
- `TryMove(Vector2I direction)` is the public API — returns `bool` for success/failure. This is what tests call directly.
- Input reads arrow keys and WASD via `_UnhandledInput`.
- Player position is authoritative in `_gridPosition` (Vector2I); the `Node2D.Position` is derived via `SnapToGrid()`.

**Communication pattern:**
- Player queries `GridMap.IsWalkable()` before moving (reads data).
- No signals emitted yet (will add when Turn Manager is introduced).

---

## Scene Tree

```
Main (Node2D) [Main.cs]
├── DungeonTileMap (TileMapLayer) [DungeonTileMap.cs]
└── Player (Node2D) [PlayerController.cs]
    └── Sprite2D
```

---

## File Layout

```
Code/
├── Grid/
│   ├── CellType.cs
│   ├── GridMap.cs
│   └── DungeonTileMap.cs
├── Player/
│   └── PlayerController.cs
└── Main.cs
tests/
├── Grid/
│   └── GridMapTest.cs
└── Player/
    └── PlayerMovementTest.cs
```

---

## Planned Systems (not yet implemented)

- **Turn Manager** — Orchestrates player turn → enemy turns.
- **Camera** — Follows the player across larger maps.
- **FOV / Visibility** — Fog of war, line of sight.
- **Enemy AI** — Pathfinding, state machines.
- **Combat** — Health, damage, death.
- **Dungeon Generation** — Procedural room/corridor placement.

---

*Last updated: 2026-04-11*
