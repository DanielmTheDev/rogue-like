using GdUnit4;
using RogueLike.Code.View.Items;
using RogueLike.Code.View.Player;
using RogueLike.Domain.Equipment;
using RogueLike.Domain.Items;
using static GdUnit4.Assertions;

namespace RogueLike.Code.View.Tests;

/// <summary>
/// A floor weapon equips onto the player (if it's better) instead of going into
/// the inventory. Calls go through the IItem.TryPickup seam, exactly as the live
/// FloorItems.CheckForPickup path does.
/// </summary>
[TestSuite]
[RequireGodotRuntime]
public class WeaponItemTest
{
    [TestCase]
    public void TryPickup_BetterWeapon_EquipsOntoPlayer_AndReportsTaken()
    {
        var player = AutoFree(new PlayerController());
        player.BaseAttackDamage = 3;
        IItem sword = AutoFree(MakeSword("Sword +1", 1));

        var taken = sword.TryPickup(player, player.Inventory);

        AssertBool(taken).IsTrue();
        AssertInt(player.AttackDamage).IsEqual(4);
        AssertObject(player.Loadout.EquippedWeapon).IsNotNull();
    }

    [TestCase]
    public void TryPickup_NotBetter_LeavesItOnFloor()
    {
        var player = AutoFree(new PlayerController());
        player.Loadout.TryEquip(new Weapon("Sword +2", 2));
        IItem sword = AutoFree(MakeSword("Sword +1", 1));

        var taken = sword.TryPickup(player, player.Inventory);

        AssertBool(taken).IsFalse();
        AssertInt(player.Loadout.DamageBonus).IsEqual(2);
    }

    private static WeaponItem MakeSword(string name, int bonus)
    {
        var sword = new WeaponItem();
        sword.Configure(new Weapon(name, bonus));
        return sword;
    }
}
