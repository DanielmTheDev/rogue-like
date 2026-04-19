# Proposed Changes: Re-implement Vim Movement Keys

## High-Level Outline
1. Define new input actions in  for 'h', 'j', 'k', 'l'.
2. Update  to include these new actions in the .
3. Verify  automatically uses the .

## Reasoning
User requested to restore Vim-style movement keys for a familiar roguelike experience.

## Architecture Impact
- **Input Actions:**  modified.
- **InputMapper:**  extended.
- **PlayerController:** No direct code changes, as it delegates to .

## Files to Modify
- project.godot
- Code/Player/InputMapper.cs

- [ ] Update project.godot with Vim key bindings
- [ ] Update InputMapper.cs
