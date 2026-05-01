# CURRENT FEATURE PLAN: Game Balance Tuning

## 1. High-Level Goal
Increase the game's difficulty and encourage more strategic play by reducing passive healing and increasing enemy density.

## 2. Step-by-Step Plan

### Part 1: Slower Passive Healing
- **[ ] Step 1.1: Add Turn Counter to `PlayerController`**
  - Create a new private field `_turnsSinceLastHeal = 0`.
- **[ ] Step 1.2: Modify Healing Logic**
  - In `PlayerController.cs`, after a turn-consuming action (move, wait), increment `_turnsSinceLastHeal`.
  - Only heal 1 HP when `_turnsSinceLastHeal` reaches `5`.
  - Reset the counter to `0` after healing.
- **[ ] Step 1.3: Update GameLog Message (Optional but Recommended)**
  - Add a log message "You feel a little better." to give feedback when the heal occurs, since it's no longer every turn.

### Part 2: Increased Enemy Spawns
- **[ ] Step 2.1: Modify `Spawner.cs`**
  - Locate the `SpawnEnemies` method.
  - Change the logic from spawning one enemy per room to spawning a random number of enemies (e.g., 2-3) per room.
- **[ ] Step 2.2: Ensure Safe Spawning**
  - Add a check to ensure we don't try to spawn more enemies than there are available floor tiles in a given room. This prevents infinite loops in small rooms.

### Part 3: Finalization
- **[ ] Step 3.1: Update `docs/SYSTEM_DESIGN.md`**
  - Briefly document the new passive healing mechanic.
- **[ ] Step 3.2: Build and Test**
  - Run `dotnet build` to ensure no errors were introduced.

## 3. Architecture Impact
- **`PlayerController`:** Becomes slightly more stateful with the addition of the healing counter.
- **`Spawner`:** The enemy spawning algorithm will be adjusted.
- The changes are localized to these two classes and do not affect the core systems in a major way.
