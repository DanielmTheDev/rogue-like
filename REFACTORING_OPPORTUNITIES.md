# Refactoring Opportunities Log

## View tests live in the game assembly (residual tradeoff — low priority)

- **Resolved:** gdUnit4 `ISceneRunner`/`[RequireGodotRuntime]` scene tests now run (see `MainSmokeTest`). They had to move *into* the game project — gdUnit4's runtime runner only connects when its generated runner scene compiles into the assembly Godot loads (the game assembly). A standalone view-test project can't host runtime tests (gdUnit4 **GD-298**, unimplemented).
- **Residual tradeoff:** `RogueLike.csproj` is now `IsTestProject` and carries the gdUnit4 + Microsoft.NET.Test.Sdk packages. They're harmless in dev/CI but are dead weight in a shipped export. *If* it ever matters, exclude the `Code/View.Tests/**` `.cs` + test packages from release export presets (or via a build configuration), or re-split once gdUnit4 GD-298 lands. **Priority: low.**

## Unify randomness on `IRng` + a single seeded run-RNG

- **Issue:** the new `IRng`/`SystemRng` seam is used by loot only. `BspDungeonGenerator` and `Spawner` still use concrete `System.Random` — and `Spawner._rng` is a **static, unseeded** instance, so enemy/potion placement isn't reproducible even when `MapSeed` is fixed. Two randomness styles now coexist.
- **Solution:** migrate `BspDungeonGenerator` + `Spawner` to take an `IRng`; have `Main` own a single seeded "run RNG" (from `MapSeed`) threaded through map gen + all spawns + loot, persisted across floors and reset on restart. Whole runs become reproducible.
- **Priority:** medium — touches every `Spawner` signature + the descend/restart lifecycle; do as a focused follow-up.

## Sword +2 reuses the +1 sprite

- **Issue:** `Sword +2` drops reuse `Assets/Sword/sword.png` (no distinct art).
- **Solution:** generate a +2 sprite via the `godot-sprite-gen` skill; let `WeaponItem`/loot pick art per weapon (needs a weapon→sprite mapping, e.g. a small catalog).
- **Priority:** low — cosmetic.

## XpReward + SightRange sit on the all-actor base (wrong abstraction)

- **Issue:** `ActorController` (base of Player) + `ICombatant` carry `XpReward` — "XP granted when this dies" — so the Player is forced to implement a meaningless value. Enemy-only concept on the all-actor base (ISP/wrong-abstraction). `SightRange` was just added on the enemy controllers (correctly not on the base), but there's no shared enemy seam to hold enemy-only data.
- **Solution:** introduce an enemy abstraction — e.g. `IXpProvider { int XpReward }` (enemies implement, Player does not); the kill hook casts `victim is IXpProvider`. Optionally a shared `EnemyControllerBase`/`IEnemy` holding `SightRange` + `XpReward`. Then (optional, later) group enemy stats into a Godot `Resource` (`EnemyStats`: health/damage/xp/sight), translated to domain like `LevelSettings`→`LootTableConfig` — keeps the Godot-free wall; excludes Player by construction.
- **Priority:** medium — next AI/stats cleanup; keep separate from feature PRs (touches `ICombatant` + every actor `.tscn`).
