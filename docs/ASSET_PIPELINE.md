# Asset Creation Pipeline

To ensure a consistent visual style and technical compatibility in this Roguelike project, follow these steps when creating new character or object sprites.

## 1. Sprite Generation (AI)
When generating a new sprite using an AI generation tool, use a prompt that specifies:
- **Perspective**: Top-down or 3/4 perspective.
- **Size**: 32x32 pixels (or a multiple thereof).
- **Style**: Retro 16-bit pixel art, limited color palette.
- **Background**: **Solid Vibrant Green (#00FF00)**. Do not ask for transparency directly in the prompt as it often generates "fake" checkerboard textures. Chromakey Green is much safer for sprites with white or light-colored details (like skeletons).

**Example Prompt:**
> "A pixel art sprite of a [ENTITY] for a roguelike dungeon crawler. 32x32 pixels, top-down perspective. Retro 16-bit pixel art style. SOLID VIBRANT GREEN (#00FF00) BACKGROUND, no shadows, no effects."

## 2. Scaling and Transparency Conversion (ImageMagick)
Since AI-generated images are usually high-res (e.g., 1024x1024), we must scale them down to our game's base size (32x32) while maintaining pixel clarity, and then strip the background.

Run the following command in the terminal:
```bash
convert path/to/input.png -scale 32x32 -fuzz 10% -transparent "#00FF00" path/to/output.png
```
- `-scale 32x32`: Uses nearest-neighbor scaling to keep pixel edges sharp.
- `-fuzz 10%`: Accounts for slight color variations in the AI output.
- `-transparent "#00FF00"`: Strips the chromakey color.

### Automated: the `godot-sprite-gen` skill
The above generate→chromakey steps are wrapped in a repeatable skill at
`.claude/skills/godot-sprite-gen/` (Gemini image API + ImageMagick). One command:
```bash
python3 .claude/skills/godot-sprite-gen/generate_sprite.py --name Sword --desc "a steel short sword, blade up"
```
It writes `Assets/<Name>/<name>.png` (32×32, transparent). Note: Gemini ignores the
requested `#00FF00` and emits its own flat backdrop, so the script **auto-detects the
real background from a corner pixel** and keys that (fuzz 20%). See the skill's `SKILL.md`.

## 3. Integration into Godot
- Save the final `.png` into the `Assets/[EntityName]/` directory.
- Create a corresponding `.tscn` in the `Scenes/` directory.
- Use `ActorController` as the base class for entities to inherit Health and Combat logic.
- Bind the sprite to the `Sprite2D` node.
- Configure the `[Export]` variables (HealthBar, BaseHealth, BaseAttackDamage) in the Godot Inspector.

## 4. Sound Effects (jsfxr)

Retro SFX are generated with **jsfxr** (a local, offline sfxr port — no API key/cost),
wrapped in the `godot-sound-gen` skill. One command:
```bash
node .claude/skills/godot-sound-gen/generate_sound.mjs --preset pickupCoin --name pickup --seed 42
```
Writes `Assets/Sounds/<name>.wav` (8-bit PCM mono, 44.1 kHz) and prints a b58 param string.
Presets: `pickupCoin`, `laserShoot`, `explosion`, `powerUp`, `hitHurt`, `jump`, `blipSelect`,
`synth`, `tone`, `click`, `random`. `--seed` makes output byte-reproducible so committed audio
regenerates from the command; `--params <b58>` reproduces/tweaks a saved sound. See the skill's
`SKILL.md`. jsfxr installs into the skill dir on first run (gitignored).

**Playback is not yet wired** — the game has no audio system (no `AudioStreamPlayer`/bus/autoload).
This pipeline only produces the asset; Godot auto-imports the `.wav` as an `AudioStreamWav`.
