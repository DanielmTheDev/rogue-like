using GdUnit4;
using Godot;
using RogueLike.Code.View.UI;
using RogueLike.Domain.Equipment;
using static GdUnit4.Assertions;

namespace RogueLike.Code.View.Tests;

/// <summary>
/// The HUD weapon label reflects the player's Loadout: shows "Unarmed" when empty
/// and the weapon name once equipped, updating live on equip changes.
/// </summary>
[TestSuite]
[RequireGodotRuntime]
public class WeaponUITest
{
    [TestCase]
    public void ShowsUnarmed_WhenNothingEquipped()
    {
        var ui = MakeUi(out var label);

        ui.Initialize(new Loadout());

        AssertString(label.Text).Contains("Unarmed");
    }

    [TestCase]
    public void ShowsWeaponName_AfterEquip()
    {
        var loadout = new Loadout();
        var ui = MakeUi(out var label);
        ui.Initialize(loadout);

        loadout.TryEquip(new Weapon("Sword +1", 1));

        AssertString(label.Text).Contains("Sword +1");
    }

    private static WeaponUI MakeUi(out Label label)
    {
        var ui = AutoFree(new WeaponUI());
        label = AutoFree(new Label());
        ui.WeaponLabel = label;
        return ui;
    }
}
