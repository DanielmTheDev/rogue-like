# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

# RogueLike — Godot 4.6 / C# (net8.0) turn-based roguelike

## Commands

```bash
dotnet build                                              # MUST be 0 errors before finishing any change
dotnet test                                               # run ALL: xUnit domain + integration (Godot-free) + gdUnit4 view incl. ISceneRunner scene tests (needs GODOT_BIN, runs headless)
dotnet test Code/Domain.Tests/RogueLike.Domain.Tests.csproj # domain + integration tests only — fast, NO Godot/GODOT_BIN
dotnet test RogueLike.csproj                              # view + scene tests only (gdUnit4, headless Godot via test.runsettings — auto-applied)
dotnet test --filter "FullyQualifiedName~TurnManagerTest" # run one suite
dotnet test --filter "FullyQualifiedName~TurnManagerTest.MethodName" # run one test
```

- **Two test projects, split by the Godot-free wall:**
  - **Domain** — `Code/Domain/RogueLike.Domain.csproj` (lib) + `Code/Domain.Tests/RogueLike.Domain.Tests.csproj` (**xUnit**, plain `Microsoft.NET.Sdk`, **no Godot ref** — the test-side of the Godot-free wall; runs via plain `dotnet test`, no `GODOT_BIN`). Includes the Godot-free **integration** layer under `Code/Domain.Tests/Integration/`.
  - **View** — view tests live **inside the game project** `RogueLike.csproj` (which is `IsTestProject` and carries the gdUnit4 packages); the `.cs` sit under `Code/View.Tests/`. They run under gdUnit4 headless. Scene tests use `[RequireGodotRuntime]` + `ISceneRunner` (e.g. `MainSmokeTest` boots `Scenes/Main.tscn`); pure-C# ones (e.g. `GridConversionsTest`) run in gdUnit4's engine-less Default runner.
  - **Why view tests are NOT a separate project:** gdUnit4's Godot-runtime runner (`[RequireGodotRuntime]`/`ISceneRunner`) only works when its generated runner scene compiles into the assembly Godot loads — the game assembly. A standalone view-test project can't host runtime tests (gdUnit4 **GD-298**, unimplemented) — it dead-ends at `Failed to connect: Connection timeout`. So view tests live in the game assembly; Domain stays separate + Godot-free. See `docs/SYSTEM_DESIGN.md`.
  - **`GODOT_BIN`** must point to a Godot 4.x **mono** binary (`/opt/Godot/Godot_v4.6.2-stable_mono_linux.x86_64`). On first use after a clean checkout, run `"$GODOT_BIN" --headless --import` once.
- **Solution folders:** `Domain/` { RogueLike.Domain, RogueLike.Domain.Tests }, `View/` { RogueLike (game + view/scene tests) }.
- Run the game from the Godot editor; main scene is `Scenes/Main.tscn`.

## Big-picture architecture

The core design split: **pure C# logic classes** hold all game rules and are unit-tested in isolation; **Godot `Node2D` controllers** are thin wrappers that handle visuals/input and delegate downward. Keep logic out of the Godot nodes.

- **Logic (pure C#, no Godot deps, tested):** `DungeonGrid` (walkability data), `GridMover` (movement validation), `CombatSystem`/`HealthController`, `EnemyAI`/`ArcherAI` (decision trees), `LineOfSight` (Bresenham), `Pathfinder` (A*), `FovMap`/`Raycaster` (`IFovAlgorithm`), `TurnManager` (Player→Enemy state machine), `EntityManager` (actor registry preventing overlap), `Inventory`, `ItemManager`, `ExperienceSystem`, `GameLog`. Decoupled from UI via **events**.
- **Controllers (Godot nodes):** `ActorController` (abstract base — owns `HealthController`, `[Export]` stats, `Die()`); `PlayerController`/`EnemyController`/`ArcherController` extend it and delegate to the logic classes. `DungeonTileMap`/`FovTileMap` are `TileMapLayer`s that render from the logical grids. `MinimapController` renders from `DungeonGrid` + `FovMap`.
- **Orchestration:** `Main.cs` drives level creation/cleanup, level transitions via stairs (player state preserved across levels), FOV recompute, and restart-on-death.
- **Map generation:** must NEVER live in `DungeonGrid`. Lives in standalone builders (e.g. `BspDungeonGenerator`) operating on a pure `DungeonGrid`.
- **Spawning:** `Spawner` (static) places entities across BSP rooms; tuned via `LevelSettings` (Godot `Resource`, editable in Inspector).

**Namespaces mirror each project's folder, relative to its `.csproj` (not the repo root) — the segment above a project's csproj never appears in the namespace:**
- **Domain** (`Code/Domain/RogueLike.Domain.csproj`) → `RogueLike.Domain.<Folder>` (the `Code/` container sits above the csproj, so it drops out).
- **View** (the Godot game `RogueLike.csproj`, pinned at the repo root by `project.godot`) → `RogueLike.Code.View.<Folder>` (its source is `Code/View/**` *relative to the root csproj*, so `Code.View` stays — and that's correct, not a bug).
- **Domain tests** (`Code/Domain.Tests/RogueLike.Domain.Tests.csproj`) → `RogueLike.Domain.Tests.<Folder>` (drops `Code` — its csproj sits inside `Code/`). **View tests** compile into the game project (`RogueLike.csproj`, pinned at repo root), so their files under `Code/View.Tests/` follow the View convention → `RogueLike.Code.View.Tests` (the `Code` segment stays, like all `RogueLike.Code.View.*`). `RogueLike.Code.View.Tests` is a child of `RogueLike.Code.View`, so `GridConversions` is in scope without an extra `using`.

Domain test classes/methods use xUnit `[Fact]`/`[Theory]` (Rider recognizes these as used). gdUnit4 `[TestSuite]`/`[TestCase]` are reflection-invoked, so Rider false-flags them "never used"; suppressed via a scoped `[Code/View.Tests/**.cs]` section in `.editorconfig` (`resharper_*_global_highlighting = none`) — scoped to the gdUnit4 folder only, not global. (Rider 2026.1 removed the External-Annotations folder UI, so editorconfig is the version-controlled, no-UI route.)

**Read `docs/SYSTEM_DESIGN.md` before any task** — it is the authoritative architecture ledger and must be updated synchronously with any change to Health, Movement, AI, or Map Generation.

## Workflow rules

- Non-trivial feature: create a task list before coding. Wait for user "OK" if the plan touches >2 core systems.
- Add an explicit "Update docs/SYSTEM_DESIGN.md" task whenever planning a feature.
- **TDD: write the failing test first, then the implementation.** No production code without a test that drove it.
- **Work in small batches** — one logical change at a time, build + test green before the next.
- **NEVER commit until the user explicitly approves the changes.** No `git commit` on your own initiative.
- **Always run `dotnet test` after every code change** — never report a task done without passing tests.
- After each feature, scan for: methods >20 lines, DRY violations, god classes, hard-coded assets, linear searches in hot paths, constructors with >4 params. Log in `REFACTORING_OPPORTUNITIES.md` (issue/solution/priority); remove entries once resolved. Refactor immediately if clean-code principles are violated.

## Domain modelling rules (rich domain)

The codebase is migrating toward **rich domain models**: behavior lives with the data it owns. Anemic data + external service mutators is an anti-pattern to reject. Apply these to all new and changed code.

- **Behavior lives with its data.** A type that owns state exposes the operations on that state; callers do NOT reach in and mutate its fields. Model the verb as a method on the entity/aggregate that owns the data (`attacker.Attack(defender)`, `item.TryPickup(actor)`), never as a free-standing service that takes the entity and mutates its internals.
- **No static `*System` services that mutate entity internals.** If you're tempted to write `SomeSystem.DoX(entity)` that writes to `entity`'s state, put `DoX()` on the entity instead. (Stateless pure-function utilities/algorithms — pathfinding, line-of-sight, raycasting — are fine; they compute, they don't mutate domain state.)
- **The domain layer is Godot-free.** Pure-C# domain types (target home: `Code/Domain/`) must not `using Godot;`. Positions/vectors/grid concepts use the project Value Objects (`GridPos`, `Direction`), never `Vector2I`, inside the domain. `Vector2I` is confined to the Godot view edge + the single conversion bridge.
- **Value Objects are immutable, equality-by-value, self-validating.** No setters; operations return new instances; invalid states are unconstructable (validate in the constructor).
- **Aggregate roots guard their own invariants.** No public setters on domain types. The aggregate rejects illegal transitions internally rather than trusting callers.
- **Godot controllers are Views.** (Target end-state.) Controllers render and forward input; they must not contain game rules. During migration a controller may still host the domain object, but no NEW rule may be added to a controller — put it on the domain type.
- **Self-review gate.** Before presenting any batch, run the rubric in `docs/DDD_REVIEW_CHECKLIST.md` (a reviewer subagent reads the diff + rubric); fix blocker/major findings before the user reviews. See `docs/SYSTEM_DESIGN.md` for the migration status table.

(Reaffirming existing rules: TDD failing-test-first, small batches build+test green, never commit without explicit user approval.)

## C# & Godot standards

- Signals for upward (child→parent) communication; `[Export]` / DI for downward. Prefer `[Export]` over `GetNode()`.
- Max 20 lines per method — refactor into sub-methods if exceeded.
- **Member ordering: public methods first, private methods below them** in every class. Reorder if a change leaves a private interleaved with publics.
- `interface`/`abstract` for multi-variant systems (`IDamageable`, `IAbility`, `ICombatant`, `IActor`, `IItem`).
- Naming: PascalCase public, `_camelCase` private fields.
- **Prefer `var` over explicit type in local declarations.** Use the explicit type only when the RHS type is not inferable/obvious from the expression.
- **Prefer collection expressions (`[]`, `[a, b]`) over `new[]{…}`/`new List<>{…}`, and target-typed `new()` when the type is apparent.** Enforced via `.editorconfig` (severity `error`) + `<EnforceCodeStyleInBuild>`: a violation **fails the build** (`dotnet build` errors on IDE0028/IDE0090/IDE0300…). Scoped to these style rules only — not broad warnings-as-errors.
- **`var` preference** is enforced (`IDE0007`, warning). **Ternary preference** (`IDE0045/IDE0046`) is a `suggestion` only — an IDE hint, deliberately NOT a build warning and NOT auto-applied, because blanket auto-conversion produces the `? false :` anti-pattern / nested ternaries that read worse. Write a ternary where it genuinely fits; `jb cleanupcode` then wraps it one-branch-per-line (`resharper_*` keys; Roslyn/`dotnet format` can't wrap ternaries).
- **Formatting gate (run at the end of every implementation chunk, before the review gate):** `dotnet format RogueLike.sln` (auto-applies whitespace + the collection-expr/var fixes), and — if the chunk added/changed any ternaries — `~/.dotnet/tools/jb cleanupcode RogueLike.sln --profile="Built-in: Reformat Code" --include="<changed files>"` to wrap them. Then `dotnet build` must be **0 warnings / 0 errors** and `dotnet test` green before presenting. (`jb` = `dotnet tool install -g JetBrains.ReSharper.GlobalTools`.)
- Call `QueueFree()` and dispose C# objects that don't inherit `GodotObject`. Physics in `_PhysicsProcess` using `delta`.

## Asset pipeline (see `docs/ASSET_PIPELINE.md`)

- Sprites: top-down, 32×32px, retro 16-bit pixel art, **solid #00FF00 background** (chromakey — do NOT request transparency).
- Prompt: *"A pixel art sprite of a [ENTITY] for a roguelike dungeon crawler. 32x32 pixels, top-down perspective. Retro 16-bit pixel art style. SOLID VIBRANT GREEN (#00FF00) BACKGROUND, no shadows, no effects."*
- Convert: `convert input.png -scale 32x32 -fuzz 10% -transparent "#00FF00" output.png`
- Place `.png` in `Assets/[EntityName]/`, create `.tscn` in `Scenes/`. Bind sprite to `Sprite2D`; set `[Export]` vars in Inspector.
