# Item System Refactoring Opportunities

## 1. **ItemManager: Spatial Lookup Optimization** [Priority: Low]
**Current:** Uses `FirstOrDefault` with linear search (O(n)).
```csharp
var item = _items.FirstOrDefault(i => i.GridPosition == position);
```
**Issue:** If we have 100+ items, this becomes slow.
**Solution:** Use `Dictionary<Vector2I, IItem>` for O(1) lookup.
**When:** Only refactor if we plan to have many (50+) items on screen at once.

---

## 2. **PlayerController: Too Many Constructor Parameters** [Priority: Medium]
**Current:** `Initialize` takes 5 parameters.
```csharp
Initialize(gridMap, entityManager, turnManager, itemManager, startPos)
```
**Issue:** Violates "Small Methods" rule. Hard to test. Will grow with more systems (XP, Quests).
**Solution:** Introduce a `GameContext` or `PlayerInitConfig` struct.
```csharp
public struct PlayerInitConfig {
    public DungeonGrid Grid;
    public EntityManager Entities;
    public TurnManager Turns;
    public ItemManager Items;
    public Vector2I StartPos;
}
player.Initialize(config);
```
**When:** Now, before we add more systems (XP, Skills).

---

## 3. **HealingPotion: Sprite in Code, Not Scene** [Priority: High]
**Current:** Sprite created in `Initialize()`.
```csharp
var sprite = new Godot.Sprite2D();
sprite.Texture = GD.Load<Texture2D>("res://Assets/Potion/potion.png");
AddChild(sprite);
```
**Issue:** Breaks the "Scene-based" pattern used by Enemies/Player. Hard to edit in Inspector.
**Solution:** Create `Scenes/HealingPotion.tscn` with Sprite2D as child. Load via `PackedScene`.
**When:** Now, before Phase 2 (Backpack UI).

---

## 4. **Spawner: Repetitive Spawn Methods** [Priority: Low]
**Current:** Separate methods for each type.
```csharp
SpawnGoblin(parent, scene, grid, entityManager, pos, index);
SpawnArcher(parent, scene, grid, entityManager, pos, index);
SpawnHealingPotion(parent, grid, itemManager, pos);
```
**Issue:** Violates DRY. Will explode with more enemy/item types.
**Solution:** Generic `Spawn<T>()` method or a Factory pattern.
**When:** After we have 5+ enemy types.

---

## 5. **Debug Feature: Health.Heal(1) on Move** [Priority: Medium]
**Current:** Player heals 1 HP every turn.
```csharp
if (TryMove(direction)) {
    Health.Heal(1); // <-- This
    _turnManager.EndPlayerTurn();
}
```
**Issue:** Makes the game too easy. Should be removed or tied to a "resting" mechanic.
**Solution:** Remove or replace with `if (IsResting) Health.Heal(1);`
**When:** Before Phase 2 (Backpack).

---

## Recommendation: Immediate Refactorings
1. **#3 (HealingPotion Scene)** - Fixes architectural inconsistency.
2. **#5 (Remove Debug Heal)** - Fixes gameplay balance.
3. **#2 (PlayerInitConfig)** - Prevents future pain when adding XP/Skills.

Should I implement these 3 now?
