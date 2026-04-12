# Godot 4.x C# Development Rules & Guardrails

## 1. The "Think Before You Code" Protocol
- **Constraint:** Before modifying or creating files, the agent MUST generate a "Proposed Changes" Artifact.
- **Content:** This artifact must include:
    1. **High-Level Outline:** List of files to be touched.
    2. **Reasoning:** Why this specific implementation was chosen over alternatives.
    3. **Architecture Impact:** How this affects the existing scene tree or signal flow.
- **Approval:** The agent must wait for user "OK" if the change affects more than 2 core systems.

## 2. Self-Documenting Life System (Architecture Ledger)
- **Constraint:** The agent must maintain a file at `docs/SYSTEM_DESIGN.md`.
- **Action:** Every time a new system (Health, Movement, AI, Map Generation) is added, refactored, or significantly altered, the agent must update this file IMMEDIATELY in the same prompt.
- **Sync Rule:** It is unacceptable to leave `docs/SYSTEM_DESIGN.md` out of sync with the live code. Update the docs synchronously with the code changes.
- **Enforcement Mechanism:** ALWAYS add an explicit checkbox to `task.md` for "Update docs/SYSTEM_DESIGN.md" during the Planning Phase so you do not forget to do it during the Execution Phase.
- **Mandatory Consult:** Before starting any task, the agent MUST read `docs/SYSTEM_DESIGN.md` to ensure the new code aligns with previous architectural decisions.
- **Goal:** Prevent "Context Drift" where the agent forgets its own previous design patterns.



## 3. C# & Godot Engineering Standards
- **Decoupling:** Use **Signals (Events)** for upward communication (Child -> Parent) and **Dependency Injection** or **Node Exports** for downward communication.
- **Small Methods:** No method should exceed 20 lines of code. If it does, refactor into sub-methods.
- **Extensibility:** Use `interface` and `abstract` classes for systems that will have multiple variations (e.g., `IDamageable`, `IAbility`).
- **Naming:** Follow standard C# PascalCase conventions. Private fields should be `_camelCase`.
- **Namespaces:** Namespaces must mirror the folder structure exactly (e.g., a file in `Code/Grid/` must be in `namespace RogueLike.Grid;`).
- **Implicit Typing:** Use `var` instead of specific types where the type is obvious from the right side of the assignment (e.g., `var direction = Vector2I.Zero;`).
- **Node Access:** Prefer `[Export]` variables over `GetNode()` to make scenes resilient to hierarchy changes.

## 4. Testing & Quality Assurance (gdUnit4Net)
- **Rule:** Every logic-heavy class (Systems, Services, Utils) must have a corresponding test suite.
- **Framework:** Use `gdUnit4Net`.
- **Location:** Tests must be placed in a `tests/` directory mirroring the `scripts/` directory structure.
- **Validation:** After writing code, the agent should attempt to run the specific test suite and report results. If tests fail, the agent must prioritize fixing the code before proceeding.

## 5. Performance & Safety
- **Memory:** Be explicit about `QueueFree()` and disposing of C# objects that don't inherit from `GodotObject`.
- **Physics:** Ensure all physics calculations happen in `_PhysicsProcess` and use `delta` correctly.