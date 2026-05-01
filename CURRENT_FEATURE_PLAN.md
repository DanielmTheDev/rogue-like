# CURRENT FEATURE PLAN: Fix Infinite Loop in "Wait Until Full Health"

## 1. High-Level Goal
Fix the game hang/infinite loop occurring when using `Shift + .` (Wait Until Full Health). Ensure the action stops if health is full or if the player is in danger.

## 2. Step-by-Step Plan

### Part 1: Reproduce & Analyze
- **[ ] Step 1.1: Create Unit Test**
  - Add a test case to `PlayerControllerTest.cs` (or create it) that simulates `WaitUntilFullHealth` while taking damage.
  - Verify that a synchronous loop without stop conditions (other than health) causes a hang if damage is sustained.

### Part 2: Fix Implementation
- **[ ] Step 2.1: Refactor `WaitUntilFullHealth` to be Non-Blocking or Safety-Guarded**
  - The current `while` loop is synchronous and blocks the engine thread.
  - **Option A (Safety Guard):** Limit the loop to a maximum number of turns (e.g., 200) and stop if an enemy is visible.
  - **Option B (State Based):** (Better but more complex) Turn "Resting" into a player state that processes one turn per frame until interrupted.
  - We'll go with **Option A+** for now: Add a `MaxTurns` safety and an `IsEnemyVisible` check inside the loop.
- **[ ] Step 2.2: Stop resting if damage is taken**
  - Record HP at start of step; if HP decreases, stop waiting.

### Part 3: Verification
- **[ ] Step 3.1: Run tests**
- **[ ] Step 3.2: Update docs/SYSTEM_DESIGN.md**

## 3. Architecture Impact
- `PlayerController.cs` logic modification. No changes to core systems.
