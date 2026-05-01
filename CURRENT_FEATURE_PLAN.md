# CURRENT FEATURE PLAN: Restart Game on Death

## 1. High-Level Goal
Allow the player to restart the game by pressing the "Enter" key after they have died.

## 2. Step-by-Step Plan

### Part 1: Player Death State
- **[ ] Step 1.1: Add `IsDead` State to `PlayerController`**
  - Add a public boolean property `IsDead` to the `PlayerController`.
  - When the player's `Health.OnDied` event is triggered, set `IsDead = true`.
- **[ ] Step 1.2: Prevent Actions While Dead**
  - In `PlayerController._UnhandledInput`, add a guard clause at the beginning: `if (IsDead) { ... }`.
  - This new block will *only* check for the restart action and will ignore all other input (movement, items, etc.).

### Part 2: Restart Logic
- **[ ] Step 2.1: Define `restart_game` Input Action**
  - In `project.godot`, add a new input action `restart_game` and map it to the `Enter` key.
- **[ ] Step 2.2: Implement Restart in `PlayerController`**
  - In the new `if (IsDead)` block, check for `event.IsActionPressed("restart_game")`.
  - If pressed, call a new public method on the `Main` node: `_main.RestartGame()`.
- **[ ] Step 2.3: Implement `Main.RestartGame()`**
  - This new method in `Main.cs` will be responsible for resetting the game state. It will essentially be a "soft" version of `_Ready()`.
  - It will perform the following actions:
    1. Reset the `_dungeonLevel` to 1.
    2. Call `DescendLevel()`. The existing `DescendLevel` method already handles cleaning up the old level and setting up a new one. We can reuse this perfectly to create the "restarted" level 1.
    3. We also need to reset the player's stats (health, inventory, experience). I'll add a `Reset()` method to the `PlayerController` for this.

### Part 3: Player State Reset
- **[ ] Step 3.1: Create `PlayerController.Reset()` Method**
  - This new public method will reset the player's character to its initial state.
  - It will:
    - Re-create the `HealthController` with `BaseHealth`.
    - Clear the `Inventory`.
    - Re-create the `ExperienceSystem`.
    - Reset `BaseAttackDamage` to its default.
    - Set `IsDead = false`.
- **[ ] Step 3.2: Call `Reset()` from `Main.RestartGame()`**
  - Before calling `DescendLevel`, the `RestartGame` method will get the player node and call `player.Reset()`.

### Part 4: Documentation & Finalization
- **[ ] Step 4.1: Update `docs/SYSTEM_DESIGN.md`**
  - Document the game restart flow.
- **[ ] Step 4.2: Build and Test**

## 3. Architecture Impact
- **`PlayerController`:** Will now have a "dead" state that alters its input handling, and a `Reset` method to restore its initial state.
- **`Main.cs`:** Will gain a `RestartGame` method that orchestrates the reset process, reusing the `DescendLevel` logic to create the new game world.
- A new input action will be added to `project.godot`.
