# CURRENT FEATURE PLAN: Tunable Enemy Spawning

## 1. High-Level Goal
Refactor the enemy spawning system to make it easily configurable from the Godot Editor. This will allow for faster iteration on game balance without requiring code changes.

## 2. Step-by-Step Plan

### Part 1: Expose Spawning Parameters
- **[ ] Step 1.1: Add a `LevelSettings` Resource**
  - Create a new C# class `LevelSettings.cs` that extends `Resource`.
  - This resource will hold all the tunable parameters for a given dungeon level.
  - It will contain `[Export]` variables like:
    - `MinEnemiesPerRoom`
    - `MaxEnemiesPerRoom`
    - `BaseEnemyCount`
- **[ ] Step 1.2: Create a Default `LevelSettings` Resource**
  - In the Godot Editor FileSystem, I will create a new resource of type `LevelSettings` (e.g., `res://Resources/default_level_settings.tres`).
  - I will set its default values to something reasonable (e.g., Min: 1, Max: 2, Base: 0).
- **[ ] Step 1.3: Use the Resource in `Main.cs`**
  - In `Main.cs`, add an `[Export]` variable to hold the `LevelSettings` resource.
  - Drag the `default_level_settings.tres` file into this slot in the Godot Inspector for the `Main` scene.

### Part 2: Refactor Spawner
- **[ ] Step 2.1: Modify `Spawner.SpawnEnemies`**
  - Change the signature to accept the `LevelSettings` resource and the `dungeonLevel`.
  - The logic for `numberOfEnemies` will change from its hard-coded formula to:
    `_rng.Next(settings.MinEnemiesPerRoom, settings.MaxEnemiesPerRoom + 1) + settings.BaseEnemyCount + dungeonLevel;`
    (or a similar formula using the resource's properties).
- **[ ] Step 2.2: Update `Main.cs` Call**
  - Update the call in `Main.SetupLevel` to pass the new `LevelSettings` resource to the spawner.

### Part 3: Tune and Finalize
- **[ ] Step 3.1: Adjust `default_level_settings.tres`**
  - As per your request for "less enemies," I will set the initial values to be more sparse (e.g., 1-2 enemies per room on level 1).
- **[ ] Step 3.2: Build and Test**

## 3. Architecture Impact
- Introduces a new `Resource`-based pattern for managing game balance parameters, which is a very common and powerful Godot practice.
- Decouples the raw numbers from the code, allowing designers (or us) to tweak the game's feel without touching the C# logic.
- `Main.cs` and `Spawner.cs` will be updated to use this new resource.
