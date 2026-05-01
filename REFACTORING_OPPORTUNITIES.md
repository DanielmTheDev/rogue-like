# Refactoring Opportunities & Proposed Changes

## 1. AGENTS.md Rule Violations (Last Commit)

### Issue: Methods Exceed 20 Lines
- **File:** `Code/Player/PlayerController.cs`
- **Method:** `ShiftMove(Vector2I direction)` is **~35 lines long**. The `while` loop contains too many distinct checks (wall, entity, path change, enemy visibility), making it hard to read and maintain.
- **Method:** `_UnhandledInput(InputEvent @event)` is **~35 lines long**. It has become a large dispatcher for various input types (wait, items, movement, shift modifiers), reducing clarity.

### Issue: Hard-coded Input Key
- **File:** `Code/Player/PlayerController.cs`
- **Violation:** The check for `keyEvent.Keycode == Key.Period` is not a scalable or user-friendly way to handle input. It should be a configurable Godot Input Action.

---

## 2. Proposed Refactoring Plan

### Refactor `ShiftMove()`
Break the complex `while` loop into a series of calls to small, single-purpose private methods.

**Before:**
```csharp
private void ShiftMove(Vector2I direction)
{
    // ... setup ...
    while (true)
    {
        if (is_wall) { break; }
        if (is_occupied) { break; }
        // ... more checks ...
    }
}
```

**After:**
```csharp
private void ShiftMove(Vector2I direction)
{
    // ... setup ...
    while (true)
    {
        if (CheckShiftMoveStopConditions(direction, ...)) break;
        
        // ... perform move ...

        if (CheckPostMoveStopConditions(...)) break;
    }
}
```
*(Note: A more detailed implementation will break this down further into even smaller helper methods for each condition.)*

### Refactor `_UnhandledInput()`
Decompose the monolithic method into a chain of responsibility.

**Before:**
```csharp
public override void _UnhandledInput(InputEvent @event)
{
    if (is_wait_key) { ... }
    else if (is_item_key) { ... }
    else if (is_move_key) { ... }
}
```

**After:**
```csharp
public override void _UnhandledInput(InputEvent @event)
{
    // ... guards ...
    if (HandleWaitActions(@event)) return;
    if (HandleItemActions(@event)) return;
    HandleMovementActions(@event);
}

// Each helper method is < 15 lines
private bool HandleWaitActions(InputEvent @event) { ... }
private bool HandleItemActions(InputEvent @event) { ... }
private void HandleMovementActions(InputEvent @event) { ... }
```

---

## 3. Proposed Input Action Change

### `.` (Period) to "wait" Action
1.  **`project.godot`:** Add a new input action named `wait` and bind it to the `.` (Period) key.
2.  **`PlayerController.cs`:** The new `HandleWaitActions` method will check for `@event.IsActionPressed("wait")` instead of the hard-coded key.

---

I will await your "OK" before applying these changes, as this is a significant refactoring of a core system.
