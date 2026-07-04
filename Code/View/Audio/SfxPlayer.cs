using System;
using System.Collections.Generic;
using Godot;

namespace RogueLike.Code.View.Audio;

/// <summary>The set of game actions that have a sound effect.</summary>
public enum Sfx
{
    Step,
    Hit,
    Death
}

/// <summary>
/// Autoload singleton that plays the game's sound effects. Registered in project.godot (as the
/// <c>Scenes/SfxPlayer.tscn</c> scene, so <see cref="Bank"/> can be assigned in the Inspector) and
/// exposes <see cref="Instance"/> everywhere (mirrors the <c>GameLog.Instance</c> pattern). Holds one
/// <see cref="AudioStreamPlayer"/> per <see cref="Sfx"/> so overlapping turn events (e.g. a hit that
/// also kills) don't cut each other off. Which .wav backs each <see cref="Sfx"/> lives in the
/// designer-editable <see cref="SfxBank"/> resource, not here.
///
/// Dark 8-bit .wavs generated via the godot-sound-gen skill, reproducible by seed:
///   step  -> --preset hitHurt   --seed 7
///   hit   -> --preset explosion --seed 3
///   death -> --preset explosion --seed 42
/// </summary>
public partial class SfxPlayer : Node
{
    public static SfxPlayer Instance { get; private set; }

    [Export] public SfxBank Bank { get; set; }

    private readonly Dictionary<Sfx, AudioStreamPlayer> _players = [];

    public override void _Ready() => Instance = this;

    public override void _ExitTree()
    {
        if (Instance == this) Instance = null;
    }

    public void Play(Sfx sfx)
    {
        EnsureLoaded();
        if (_players.TryGetValue(sfx, out var player) && IsInstanceValid(player))
            player.Play();
    }

    /// <summary>The stream backing a given sfx, or null if unmapped/unloaded. Used by tests.</summary>
    public AudioStream StreamFor(Sfx sfx)
    {
        EnsureLoaded();
        return _players.TryGetValue(sfx, out var player) ? player.Stream : null;
    }

    private void EnsureLoaded()
    {
        if (_players.Count > 0 || Bank == null) return;
        foreach (var sfx in Enum.GetValues<Sfx>())
        {
            var player = new AudioStreamPlayer { Stream = Bank.StreamFor(sfx) };
            AddChild(player);
            _players[sfx] = player;
        }
    }
}
