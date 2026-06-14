# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

# RogueLike — Godot 4.6 / C# (net8.0) turn-based roguelike

## Commands

```bash
dotnet build                                              # MUST be 0 errors before finishing any change
dotnet test                                               # run all gdUnit4 tests (headless Godot via test.runsettings)
dotnet test --filter "FullyQualifiedName~TurnManagerTest" # run one suite
dotnet test --filter "FullyQualifiedName~TurnManagerTest.MethodName" # run one test
```

- Tests run headless Godot — configured in `test.runsettings` (`--headless`, stdout capture).
- Run the game from the Godot editor; main scene is `Scenes/Main.tscn`.

## Big-picture architecture

The core design split: **pure C# logic classes** hold all game rules and are unit-tested in isolation; **Godot `Node2D` controllers** are thin wrappers that handle visuals/input and delegate downward. Keep logic out of the Godot nodes.

- **Logic (pure C#, no Godot deps, tested):** `DungeonGrid` (walkability data), `GridMover` (movement validation), `CombatSystem`/`HealthController`, `EnemyAI`/`ArcherAI` (decision trees), `LineOfSight` (Bresenham), `Pathfinder` (A*), `FovMap`/`Raycaster` (`IFovAlgorithm`), `TurnManager` (Player→Enemy state machine), `EntityManager` (actor registry preventing overlap), `Inventory`, `ItemManager`, `ExperienceSystem`, `GameLog`. Decoupled from UI via **events**.
- **Controllers (Godot nodes):** `ActorController` (abstract base — owns `HealthController`, `[Export]` stats, `Die()`); `PlayerController`/`EnemyController`/`ArcherController` extend it and delegate to the logic classes. `DungeonTileMap`/`FovTileMap` are `TileMapLayer`s that render from the logical grids. `MinimapController` renders from `DungeonGrid` + `FovMap`.
- **Orchestration:** `Main.cs` drives level creation/cleanup, level transitions via stairs (player state preserved across levels), FOV recompute, and restart-on-death.
- **Map generation:** must NEVER live in `DungeonGrid`. Lives in standalone builders (e.g. `BspDungeonGenerator`) operating on a pure `DungeonGrid`.
- **Spawning:** `Spawner` (static) places entities across BSP rooms; tuned via `LevelSettings` (Godot `Resource`, editable in Inspector).

`namespace RogueLike.<Folder>` mirrors the `Code/` tree exactly. Tests in `tests/` mirror `Code/`.

**Read `docs/SYSTEM_DESIGN.md` before any task** — it is the authoritative architecture ledger and must be updated synchronously with any change to Health, Movement, AI, or Map Generation.

## Workflow rules

- Non-trivial feature: create a task list before coding. Wait for user "OK" if the plan touches >2 core systems.
- Add an explicit "Update docs/SYSTEM_DESIGN.md" task whenever planning a feature.
- **TDD: write the failing test first, then the implementation.** No production code without a test that drove it.
- **Work in small batches** — one logical change at a time, build + test green before the next.
- **NEVER commit until the user explicitly approves the changes.** No `git commit` on your own initiative.
- **Always run `dotnet test` after every code change** — never report a task done without passing tests.
- After each feature, scan for: methods >20 lines, DRY violations, god classes, hard-coded assets, linear searches in hot paths, constructors with >4 params. Log in `REFACTORING_OPPORTUNITIES.md` (issue/solution/priority); remove entries once resolved. Refactor immediately if clean-code principles are violated.

## C# & Godot standards

- Signals for upward (child→parent) communication; `[Export]` / DI for downward. Prefer `[Export]` over `GetNode()`.
- Max 20 lines per method — refactor into sub-methods if exceeded.
- `interface`/`abstract` for multi-variant systems (`IDamageable`, `IAbility`, `ICombatant`, `IActor`, `IItem`).
- Naming: PascalCase public, `_camelCase` private fields.
- **Prefer `var` over explicit type in local declarations.** Use the explicit type only when the RHS type is not inferable/obvious from the expression.
- Call `QueueFree()` and dispose C# objects that don't inherit `GodotObject`. Physics in `_PhysicsProcess` using `delta`.

## Asset pipeline (see `docs/ASSET_PIPELINE.md`)

- Sprites: top-down, 32×32px, retro 16-bit pixel art, **solid #00FF00 background** (chromakey — do NOT request transparency).
- Prompt: *"A pixel art sprite of a [ENTITY] for a roguelike dungeon crawler. 32x32 pixels, top-down perspective. Retro 16-bit pixel art style. SOLID VIBRANT GREEN (#00FF00) BACKGROUND, no shadows, no effects."*
- Convert: `convert input.png -scale 32x32 -fuzz 10% -transparent "#00FF00" output.png`
- Place `.png` in `Assets/[EntityName]/`, create `.tscn` in `Scenes/`. Bind sprite to `Sprite2D`; set `[Export]` vars in Inspector.
