# RogueLike Project Context & Rules
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
- **Domain Refactoring:** Do not be afraid to heavily refactor the code if deeper insight into the domain model is acquired. It is always better to restructure cleanly than to hack new features into rigid, outdated domain models.
- **Decoupling:** Use **Signals (Events)** for upward communication (Child -> Parent) and **Dependency Injection** or **Node Exports** for downward communication.
- **Small Methods:** No method should exceed 20 lines of code. If it does, refactor into sub-methods.
- **Extensibility:** Use `interface` and `abstract` classes for systems that will have multiple variations (e.g., `IDamageable`, `IAbility`).
- **Naming:** Follow standard C# PascalCase conventions. Private fields should be `_camelCase`.
- **Namespaces:** Namespaces must mirror the folder structure exactly (e.g., a file in `Code/Grid/` must be in `namespace RogueLike.Grid;`).
- **Implicit Typing:** Use `var` instead of specific types where the type is obvious from the right side of the assignment (e.g., `var direction = Vector2I.Zero;`).
- **Node Access:** Prefer `[Export]` variables over `GetNode()` to make scenes resilient to hierarchy changes.

## 4. Testing & Quality Assurance (gdUnit4Net)
- **Mandatory Final Build Check:** You MUST strictly run `dotnet build` via the terminal and confirm 0 errors AFTER completing all code modifications, and BEFORE finalizing your turn or writing a Walkthrough artifact. Never assume a project builds just because the unit tests compiled earlier in the turn.
- **Rule:** Every logic-heavy class (Systems, Services, Utils) must have a corresponding test suite.
- **Framework:** Use `gdUnit4Net`.
- **Location:** Tests must be placed in a `tests/` directory mirroring the `scripts/` directory structure.
- **Validation:** After writing code, the agent should attempt to run the specific test suite and report results. If tests fail, the agent must prioritize fixing the code before proceeding.

## 5. Performance & Safety
- **Memory:** Be explicit about `QueueFree()` and disposing of C# objects that don't inherit from `GodotObject`.
- **Physics:** Ensure all physics calculations happen in `_PhysicsProcess` and use `delta` correctly.
# Asset Creation Pipeline

To ensure a consistent visual style and technical compatibility in this Roguelike project, follow these steps when creating new character or object sprites.

## 1. Sprite Generation (AI)
When generating a new sprite using an AI generation tool, use a prompt that specifies:
- **Perspective**: Top-down or 3/4 perspective.
- **Size**: 32x32 pixels (or a multiple thereof).
- **Style**: Retro 16-bit pixel art, limited color palette.
- **Background**: **Solid Vibrant Green (#00FF00)**. Do not ask for transparency directly in the prompt as it often generates "fake" checkerboard textures. Chromakey Green is much safer for sprites with white or light-colored details (like skeletons).

**Example Prompt:**
> "A pixel art sprite of a [ENTITY] for a roguelike dungeon crawler. 32x32 pixels, top-down perspective. Retro 16-bit pixel art style. SOLID VIBRANT GREEN (#00FF00) BACKGROUND, no shadows, no effects."

## 2. Scaling and Transparency Conversion (ImageMagick)
Since AI-generated images are usually high-res (e.g., 1024x1024), we must scale them down to our game's base size (32x32) while maintaining pixel clarity, and then strip the background.

Run the following command in the terminal:
```bash
convert path/to/input.png -scale 32x32 -fuzz 10% -transparent "#00FF00" path/to/output.png
```
- `-scale 32x32`: Uses nearest-neighbor scaling to keep pixel edges sharp.
- `-fuzz 10%`: Accounts for slight color variations in the AI output.
- `-transparent "#00FF00"`: Strips the chromakey color.

## 3. Integration into Godot
- Save the final `.png` into the `Assets/[EntityName]/` directory.
- Create a corresponding `.tscn` in the `Scenes/` directory.
- Use `ActorController` as the base class for entities to inherit Health and Combat logic.
- Bind the sprite to the `Sprite2D` node.
- Configure the `[Export]` variables (HealthBar, BaseHealth, BaseAttackDamage) in the Godot Inspector.
