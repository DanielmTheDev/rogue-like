---
name: godot-sound-gen
description: Use when adding a new sound effect (sfx) to this Godot roguelike — a pickup, hit, explosion, jump, powerup, laser, or UI blip — and you need to generate the .wav. Produces a retro 8-bit .wav in Assets/Sounds/ via jsfxr (local, offline, no API key). Keywords sfx sound audio wav jsfxr retro sfxr.
---

# Godot Sound Generation

Generate a game-ready retro SFX `.wav` for this repo via **jsfxr** — a local, offline
JS port of sfxr. **No API key, no cost, no network** (after the one-time install), and
byte-reproducible with a seed. Sibling of `godot-sprite-gen`: one repeatable command →
asset in `Assets/`.

## When to Use

- Adding any short game sound effect: pickup, hit/hurt, explosion, jump, powerup, laser, UI blip.
- Node is on PATH (v20+). jsfxr auto-installs into the skill dir on first run.

## Run It

```bash
node .claude/skills/godot-sound-gen/generate_sound.mjs --preset pickupCoin --name pickup --seed 42
```

Writes `Assets/Sounds/pickup.wav` and prints a **b58 param string**. First run does a
one-time `npm install jsfxr` in the skill dir. Run `--help` for all flags.

## Conventions

- **Presets** (`--preset`): `pickupCoin`, `laserShoot`, `explosion`, `powerUp`, `hitHurt`,
  `jump`, `blipSelect`, `synth`, `tone`, `click`, `random`. Each is randomized per run.
- **`--seed <int>`**: makes generation **byte-reproducible** — same preset + seed → identical
  `.wav`. Use it so committed audio is regenerable from the command, not an opaque blob.
- **`--params <b58>`**: paste back the printed b58 string to reproduce/tweak a sound you
  liked (also loads on [sfxr.me](https://sfxr.me)). Reproduces the sound faithfully; for a
  *byte-identical* rebuild use the original `--seed` instead (b58 quantizes float params).
- **Output**: flat in `Assets/Sounds/<name>.wav`, 8-bit PCM mono @ 44.1 kHz.

## After Generating

- Godot auto-imports the `.wav` → `AudioStreamWav` on next editor focus (creates `.wav.import`).
- **Playing it in-game is NOT wired up** — the project has no audio system yet (no
  `AudioStreamPlayer`, bus, or autoload). Adding playback (an `SfxPlayer` autoload hooking
  `GameLog`/combat/death events) is separate work with its own tests. This skill only makes
  the asset.

## Common Mistakes

- **`jsfxr not found` then a network call**: expected on first run (one-time install). If it
  fails, `cd .claude/skills/godot-sound-gen && npm install` manually.
- **`node: command not found`**: node isn't on PATH (nvm shells). `nvm use` or use the full path.
- **Silent / empty .wav from `--params`**: fixed in-script (b58 omits render config; defaults
  are backfilled). If you hand-edit params, keep `sample_rate`/`sample_size`/`sound_vol` set.
- **Sound too long/quiet/wrong feel**: try another `--seed` or preset — generation is random.
