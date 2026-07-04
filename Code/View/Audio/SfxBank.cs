using Godot;

namespace RogueLike.Code.View.Audio;

/// <summary>
/// Inspector-authored catalog mapping each <see cref="Sfx"/> to its audio stream. A Godot
/// <see cref="Resource"/> (like <c>LevelSettings</c>) so a designer can add/swap sounds in a
/// <c>.tres</c> without touching or recompiling C#. Held by <see cref="SfxPlayer"/> via [Export].
/// </summary>
[GlobalClass]
public partial class SfxBank : Resource
{
    [Export] public AudioStream Step { get; set; }
    [Export] public AudioStream Hit { get; set; }
    [Export] public AudioStream Death { get; set; }

    public AudioStream StreamFor(Sfx sfx) => sfx switch
    {
        Sfx.Step => Step,
        Sfx.Hit => Hit,
        Sfx.Death => Death,
        _ => null
    };
}
