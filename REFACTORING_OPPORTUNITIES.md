# Refactoring Opportunities Log

## View tests live in the game assembly (residual tradeoff — low priority)

- **Resolved:** gdUnit4 `ISceneRunner`/`[RequireGodotRuntime]` scene tests now run (see `MainSmokeTest`). They had to move *into* the game project — gdUnit4's runtime runner only connects when its generated runner scene compiles into the assembly Godot loads (the game assembly). A standalone view-test project can't host runtime tests (gdUnit4 **GD-298**, unimplemented).
- **Residual tradeoff:** `RogueLike.csproj` is now `IsTestProject` and carries the gdUnit4 + Microsoft.NET.Test.Sdk packages. They're harmless in dev/CI but are dead weight in a shipped export. *If* it ever matters, exclude the `Code/View.Tests/**` `.cs` + test packages from release export presets (or via a build configuration), or re-split once gdUnit4 GD-298 lands. **Priority: low.**

## TEMP hardcoded weapon spawn (loot bring-up)

- **Issue:** `Main.SetupLevel` hardcodes spawning a Sword +1 / Sword +2 next to the player on every level (marked `// TEMP`), and both reuse `Assets/Sword/sword.png`. This was deliberate scaffolding to make the loot system testable/playable.
- **Solution:** replace with data-driven loot placement (weapon table + drop rules / room placement, à la `SpawnPotions`), and give Sword +2 its own sprite. Remove the `WalkableNeighbours` helper if no longer needed.
- **Priority:** medium — do when the loot system grows past rudimentary.
