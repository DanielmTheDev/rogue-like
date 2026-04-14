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

## 3. Integration into Godot
- Save the final `.png` into the `Assets/[EntityName]/` directory.
- Create a corresponding `.tscn` in the `Scenes/` directory.
- Use `ActorController` as the base class for entities to inherit Health and Combat logic.
- Bind the sprite to the `Sprite2D` node.
- Configure the `[Export]` variables (HealthBar, BaseHealth, BaseAttackDamage) in the Godot Inspector.
