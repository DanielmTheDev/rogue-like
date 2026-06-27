using Godot;

namespace RogueLike.Code.View.Resources;

/// <summary>
/// A Godot Resource to hold tunable parameters for dungeon level generation.
/// This allows for easy game balance adjustments from the Inspector without changing code.
/// </summary>
[GlobalClass]
public partial class LevelSettings : Resource
{
    [Export(PropertyHint.Range, "0,10,1")]
    public int MinEnemiesPerRoom { get; set; } = 1;

    [Export(PropertyHint.Range, "1,10,1")]
    public int MaxEnemiesPerRoom { get; set; } = 2;

    // This value is added to the dungeon level to determine the final enemy count.
    // e.g., Base=0, Level=1 -> 1-2 enemies. Base=2, Level=1 -> 3-4 enemies.
    [Export(PropertyHint.Range, "0,10,1")]
    public int BaseEnemyCountModifier { get; set; } = 0;

    [Export(PropertyHint.Range, "0,5,0.1")]
    public float DifficultyScaling { get; set; } = 0.5f;

    // Fixed map seed for reproducible layouts (debugging/testing). -1 = random each generation.
    [Export]
    public int MapSeed { get; set; } = -1;
}
