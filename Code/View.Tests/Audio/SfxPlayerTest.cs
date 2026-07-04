using System;
using GdUnit4;
using Godot;
using RogueLike.Code.View.Audio;
using static GdUnit4.Assertions;

namespace RogueLike.Code.View.Tests.Audio;

/// <summary>
/// Routing tests for the <see cref="SfxPlayer"/> autoload. Audio playback itself can't be asserted
/// headless, so we assert the mapping instead: given the production <see cref="SfxBank"/>, every
/// <see cref="Sfx"/> resolves to a loaded stream and Play never throws. The autoload isn't loaded by
/// ISceneRunner, so we build the player + bank directly. Requires the Godot runtime for GD.Load.
/// </summary>
[TestSuite]
[RequireGodotRuntime]
public class SfxPlayerTest
{
    private const string BankPath = "res://Resources/SfxBank.tres";

    [TestCase]
    public void EverySfx_ResolvesToALoadedStream()
    {
        var sfx = AutoFree(new SfxPlayer { Bank = GD.Load<SfxBank>(BankPath) });

        foreach (var id in Enum.GetValues<Sfx>())
            AssertObject(sfx.StreamFor(id))
                .OverrideFailureMessage($"Sfx.{id} has no loaded AudioStream")
                .IsNotNull();
    }

    [TestCase]
    public void Play_DoesNotThrow_ForEverySfx()
    {
        var sfx = AutoFree(new SfxPlayer { Bank = GD.Load<SfxBank>(BankPath) });

        foreach (var id in Enum.GetValues<Sfx>())
            sfx.Play(id);
    }
}
