using System.Threading.Tasks;
using GdUnit4;
using RogueLike.Code.View.Player;
using RogueLike.Domain.Equipment;
using static GdUnit4.Assertions;

namespace RogueLike.Code.View.Tests;

/// <summary>
/// Verifies the view-side glue: a PlayerController's attack damage is its base
/// damage plus the bonus from its equipped weapon (Loadout). The "is it better?"
/// rule itself is covered Godot-free in LoadoutTest.
/// </summary>
[TestSuite]
[RequireGodotRuntime]
public class PlayerLoadoutTest
{
    [TestCase]
    public void AttackDamage_Unarmed_IsBaseDamage()
    {
        var player = AutoFree(new PlayerController());
        player.BaseAttackDamage = 3;

        AssertInt(player.AttackDamage).IsEqual(3);
    }

    [TestCase]
    public void AttackDamage_IncludesEquippedWeaponBonus()
    {
        var player = AutoFree(new PlayerController());
        player.BaseAttackDamage = 3;

        player.Loadout.TryEquip(new Weapon("Sword +1", 1));

        AssertInt(player.AttackDamage).IsEqual(4);
    }

    // Reset() touches the player's fully-wired systems (inventory, experience,
    // health), so it needs the live scene rather than a bare node.
    [TestCase]
    public async Task Reset_ClearsEquippedWeapon()
    {
        using var runner = ISceneRunner.Load("res://Scenes/Main.tscn", true);
        await runner.AwaitIdleFrame();
        var player = runner.Scene().GetNode<PlayerController>("Player");
        player.Loadout.TryEquip(new Weapon("Sword +1", 1));

        player.Reset();

        AssertObject(player.Loadout.EquippedWeapon).IsNull();
    }
}
