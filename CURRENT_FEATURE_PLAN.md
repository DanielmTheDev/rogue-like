# CURRENT FEATURE PLAN: Experience & Leveling System

## 1. High-Level Goal
Implement a system where the player can gain experience points (XP) by defeating enemies, level up, and become stronger. The player's progress will be displayed via a UI element on the main screen.

## 2. Step-by-Step Plan

### Part 1: Core XP Logic (Pure C# & Unit Tested)
- **[ ] Step 1.1: Create `ExperienceSystem.cs`**
  - A pure C# class to handle all XP and leveling logic.
  - It will manage `CurrentXP`, `CurrentLevel`, and `XPForNextLevel`.
  - It will have an `AddXP(int amount)` method that processes XP gains and triggers level ups.
  - It will include events like `OnLevelUp` and `OnXPChanged` for the UI and Player to subscribe to.
  - XP requirement will scale with each level.
- **[ ] Step 1.2: Create Unit Tests for `ExperienceSystem`**
  - Create `tests/Systems/ExperienceSystemTest.cs`.
  - Test cases:
    - Gaining XP without leveling up.
    - Gaining exact XP to level up.
    - Gaining enough XP to level up multiple times at once.
    - XP carrying over correctly after a level up.

### Part 2: Integration with Gameplay
- **[ ] Step 2.1: Add `XpReward` to Enemies**
  - Add an `[Export] public int XpReward { get; private set; } = 50;` property to `ActorController.cs`.
  - Set appropriate values for Goblin and Archer in their `.tscn` files.
- **[ ] Step 2.2: Grant XP on Kill**
  - Modify `CombatSystem.cs`. When an enemy is defeated, grant its `XpReward` to the player.
- **[ ] Step 2.3: Integrate `ExperienceSystem` with `PlayerController`**
  - Add an `ExperienceSystem` instance to the `PlayerController`.
  - Subscribe to the `OnLevelUp` event to increase the player's `BaseAttackDamage` and `Health.MaxHp`.

### Part 3: User Interface
- **[ ] Step 3.1: Create `ExperienceUI.tscn` Scene**
  - A `CanvasLayer` scene positioned in the top-left.
  - Will contain a `ProgressBar` (styled to be purple) and a `Label` for the level number.
- **[ ] Step 3.2: Create `ExperienceUI.cs` Controller Script**
  - A script to manage the UI elements.
  - It will have an `Initialize(ExperienceSystem expSystem)` method.
  - It will subscribe to the `OnXPChanged` and `OnLevelUp` events to keep the display updated.
- **[ ] Step 3.3: Wire up the UI in `Main.tscn` and `Main.cs`**
  - Add the `ExperienceUI` scene to `Main.tscn`.
  - In `Main.cs`, get the player's `ExperienceSystem` and pass it to the UI's `Initialize` method.

### Part 4: Documentation & Finalization
- **[ ] Step 4.1: Update `docs/SYSTEM_DESIGN.md`**
  - Document the new `ExperienceSystem` and its role in player progression.
- **[ ] Step 4.2: Final Build & Test**
  - Run `dotnet build` and all unit tests to ensure everything is working correctly.

## 3. Architecture Impact
- A new pure C# `ExperienceSystem` will be created, following our decoupled architecture pattern.
- The `CombatSystem` will be updated to interact with the player's `ExperienceSystem`.
- A new UI scene and controller will be created to visualize the system.
