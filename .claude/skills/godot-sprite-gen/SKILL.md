---
name: godot-sprite-gen
description: Use when adding a new 2D game sprite/asset to this Godot roguelike (a new enemy, item, weapon, prop) and you need to generate the .png — produces a 32x32 transparent pixel-art sprite via Gemini image API + ImageMagick chromakey, following docs/ASSET_PIPELINE.md.
---

# Godot Sprite Generation

Generate a game-ready sprite for this repo: AI image on a solid green
background, then chromakey it out to a 32×32 transparent PNG in `Assets/`.
Implements `docs/ASSET_PIPELINE.md` as one repeatable command.

## When to Use

- Adding any new sprite: enemy, item, weapon, consumable, prop.
- You have `GEMINI_API_KEY` in the environment and ImageMagick (`convert`).

## Run It

```bash
GEMINI_API_KEY=$GEMINI_API_KEY \
python3 .claude/skills/godot-sprite-gen/generate_sprite.py \
  --name Sword \
  --desc "a steel short sword, blade pointing up, single weapon centered"
```

Writes `Assets/Sword/sword.png` (transparent, 32×32). Re-run with
`--keep-raw ~/Downloads/claude-tmp/sword_raw.png` to save the un-keyed image so
you can re-tune the chromakey (`--fuzz`) without re-spending an API call. Run
`--help` for all flags (`--size`, `--out-dir`, `--model`, `--fuzz`).

## Conventions (enforced by the script)

- **Background:** we ask for solid `#00FF00`, but Gemini ignores the exact hex
  and emits its own flat backdrop (often a muted lime ~`#ACD160`). So the script
  **auto-detects the real background from a corner pixel** and keys *that* out —
  override with `--bg "srgb(r,g,b)"` if a corner isn't background. Never request
  transparency in the prompt — AI fakes it with checkerboard.
- **Fuzz:** defaults to `20%` — needed to remove the anti-aliased fringe between
  subject and backdrop (10% leaves a colored halo, 30% erodes the sprite).
- **Size:** 32×32, nearest-neighbour scale (sharp pixels).
- **Style:** retro 16-bit pixel art, top-down, limited palette, no shadows/text.

## After Generating

1. Inspect the PNG — confirm a clean subject and transparent (not green) edges.
   If green fringe remains, re-key the raw with a higher `--fuzz` (e.g. `15%`).
2. Godot auto-imports the PNG on next editor focus (creates `.png.import`).
3. Create a `.tscn` in `Scenes/` binding it to a `Sprite2D` — mirror an existing
   one (`Scenes/HealingPotion.tscn` for items, `Scenes/Player.tscn` for actors).

## Common Mistakes

- **Colored halo around sprite:** fuzz too low for the fringe. Re-key the
  `--keep-raw` file with `--fuzz 25%`.
- **Sprite eroded / chopped:** fuzz too high, or the subject shares the backdrop
  hue. Lower `--fuzz`, or pick a backdrop color far from the subject via prompt.
- **Subject too small / off-center:** add "single … centered, fills the frame"
  to `--desc` and re-run.
- **Wrong model / 404:** override with `--model` (default `gemini-2.5-flash-image`).
