# DDD Review Checklist

Machine-usable rubric for the **self-review gate**. Before any batch is presented to the user, a reviewer subagent receives this rubric + the batch diff + the current migration phase, and returns findings as:

```
path:line: <severity>: <problem>. <fix>.
```

No praise, no scope creep. Severities: **blocker** (must fix before presenting), **major** (must fix unless explicitly deferred with reason), **minor** (note; fix if cheap).

## Rules

| # | Anti-pattern | Severity | Notes |
|---|--------------|----------|-------|
| 1 | `using Godot;` (or any Godot type: `Vector2I`, `Node`, `Node2D`, `PackedScene`, `Mathf`, `[Export]`, signals) inside a `Code/Domain/` file | **blocker** | The domain layer must be Godot-free. Use `GridPos`/`Direction` and `System.Math`. |
| 2 | A static (or stateless) service/`*System` that takes an entity and **mutates that entity's internals** | **blocker** | Move the verb onto the entity that owns the state (`attacker.Attack(defender)`). Pure compute-only utilities (pathfinding, LOS, raycast) are exempt — they must not write domain state. |
| 3 | Public setter (`{ set; }`/`{ get; set; }`) or public mutable field on a domain Value Object or aggregate | **major** | VOs are immutable (operations return new instances). Aggregates expose behavior methods, not setters. |
| 4 | Primitive obsession on a domain concept that has a VO (position as `int x,y`/`Vector2I`; damage/hp/xp as bare `int`) | **major** | Use `GridPos`, `Direction`, `Damage`, `Health`, `XpAmount`. |
| 5 | Behavior placed on a service when it could live on the entity that owns the data it touches | **major** | Relocate to the owning aggregate. |
| 6 | Game **rule** logic added to a Godot controller/view (damage calc, turn cadence, level-up, AI decisions) | **major** during migration / **blocker** post-Phase-3 | Controllers render + forward input only. Put the rule on the domain type. |
| 7 | A domain event payload carrying a Godot type or a node reference | **major** | Event records carry only VOs / primitives / domain types. |
| 8 | Invalid state is constructable (VO/aggregate ctor doesn't reject illegal args) | **major** | Validate invariants in the constructor; throw on violation. |
| 9 | A VO is a mutable `struct`/`class` instead of `readonly record struct` (no value equality) | **major** | Use `readonly record struct` unless there's a documented reason. |
| 10 | Aggregate exposes its internal mutable collection directly (e.g. returns the backing `List<>`) | **major** | Return read-only views; mutate only through methods. |
| 11 | Method > 20 lines, or constructor with > 4 params | **minor** | Extract sub-methods / introduce a parameter object (e.g. `TurnContext`). |
| 12 | No driving test for new/changed production behavior (TDD violation) | **major** | A failing test must have driven the change. |
| 13 | Presentation strings (BBCode `[color=...]`, UI text) produced inside domain logic | **minor** | Emit structured events; format in the View. |

## Phase awareness

- During **Phase 1–2**, controllers still host domain objects and `Vector2I` still appears outside `Code/Domain/` — rules 1 and 6 apply only to *new* domain code and *newly added* controller rules, not to not-yet-migrated legacy.
- After **Phase 3**, rule 6 escalates to blocker and controllers must hold no rules at all.

## Output contract

Return only the findings list (one line each, format above), then a final line: `VERDICT: PASS` (no blocker/major) or `VERDICT: FAIL`. If FAIL, the gate fixes and re-reviews (cap ~3 loops) before presenting to the user.
