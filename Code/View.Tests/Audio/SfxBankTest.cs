using System;
using GdUnit4;
using Godot;
using RogueLike.Code.View.Audio;
using static GdUnit4.Assertions;

namespace RogueLike.Code.View.Tests.Audio;

/// <summary>
/// Guards the authored <c>SfxBank.tres</c>: every <see cref="Sfx"/> must be wired to a stream in the
/// Inspector-edited resource. Catches an unassigned slot or an enum value added without a mapping —
/// which would otherwise fail silently (no sound) at runtime. Requires the Godot runtime for GD.Load.
/// </summary>
[TestSuite]
[RequireGodotRuntime]
public class SfxBankTest
{
    [TestCase]
    public void ProductionBank_MapsEverySfx()
    {
        var bank = GD.Load<SfxBank>("res://Resources/SfxBank.tres");
        AssertObject(bank).OverrideFailureMessage("SfxBank.tres failed to load").IsNotNull();

        foreach (var id in Enum.GetValues<Sfx>())
            AssertObject(bank.StreamFor(id))
                .OverrideFailureMessage($"SfxBank has no stream assigned for Sfx.{id}")
                .IsNotNull();
    }
}
