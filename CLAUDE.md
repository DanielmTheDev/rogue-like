# RogueLike — Claude Instructions
# Godot 4.x / C# Project

## Session Start
- Read `docs/SYSTEM_DESIGN.md` before any task to avoid context drift.

## Planning
- For any non-trivial feature: create a task list via the built-in Tasks tool before writing code.
- Wait for user "OK" if the plan affects more than 2 core systems.

## Architecture Ledger
- Maintain `docs/SYSTEM_DESIGN.md`. Update it synchronously with any code change to Health, Movement, AI, or Map Generation systems.
- Add "Update docs/SYSTEM_DESIGN.md" as an explicit task whenever planning a feature.

## C# & Godot Standards
- Signals for upward (child→parent) communication; `[Export]` / DI for downward.
- Max 20 lines per method — refactor into sub-methods if exceeded.
- Use `interface` and `abstract` for multi-variant systems (`IDamageable`, `IAbility`, etc.).
- Naming: PascalCase for public members, `_camelCase` for private fields.
- Namespaces mirror folder structure exactly (e.g. `Code/Grid/` → `namespace RogueLike.Grid`).
- Use `var` where type is obvious from the RHS.
- Prefer `[Export]` over `GetNode()`.

## Testing (gdUnit4Net)
- Every logic-heavy class needs a test suite in `tests/` mirroring `Code/` structure.
- Run `dotnet build` after all code changes — confirm 0 errors before finishing.
- Run relevant tests and fix failures before proceeding.

## Performance & Safety
- Call `QueueFree()` and dispose C# objects that don't inherit `GodotObject`.
- Physics calculations in `_PhysicsProcess` with `delta`.

## Refactoring
- After each feature, scan for: methods >20 lines, DRY violations, god classes, hard-coded assets, linear searches in hot paths, constructors with >4 params.
- Log opportunities in `REFACTORING_OPPORTUNITIES.md` (issue, solution, priority). Remove entries once resolved.
- Refactor immediately if clean code principles are violated; otherwise before the next feature.

## Asset Pipeline
- Sprites: top-down, 32×32px, retro 16-bit pixel art, **solid #00FF00 background** (chromakey — do NOT request transparency directly).
- Prompt template: *"A pixel art sprite of a [ENTITY] for a roguelike dungeon crawler. 32x32 pixels, top-down perspective. Retro 16-bit pixel art style. SOLID VIBRANT GREEN (#00FF00) BACKGROUND, no shadows, no effects."*
- Convert: `convert input.png -scale 32x32 -fuzz 10% -transparent "#00FF00" output.png`
- Place `.png` in `Assets/[EntityName]/`, create `.tscn` in `Scenes/`.
- Base class for entities: `ActorController`. Bind sprite to `Sprite2D`. Set `[Export]` vars in Inspector.
