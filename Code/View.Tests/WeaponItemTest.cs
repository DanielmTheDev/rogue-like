using GdUnit4;
using RogueLike.Code.View.Items;
using RogueLike.Domain.Equipment;
using static GdUnit4.Assertions;
using PlayerActor = RogueLike.Domain.Actors.Player;

namespace RogueLike.Code.View.Tests;

/// <summary>
/// A floor weapon equips onto the player (if it's better) instead of going into the inventory. It opts
/// in via <see cref="IEquippable"/>; the Player aggregate owns the acquire — the same path the live
/// FloorItems.CheckForPickup → Player.TryPickup uses.
/// </summary>
[TestSuite]
[RequireGodotRuntime]
public class WeaponItemTest
{
    [TestCase]
    public void TryPickup_BetterWeapon_EquipsOntoPlayer_AndReportsTaken()
    {
        var player = new PlayerActor(maxHealth: 20, baseAttack: 3);
        var sword = AutoFree(MakeSword("Sword +1", 1));

        var taken = player.TryPickup(sword);

        AssertBool(taken).IsTrue();
        AssertInt(player.AttackDamage).IsEqual(4);
        AssertObject(player.Loadout.EquippedWeapon).IsNotNull();
    }

    [TestCase]
    public void TryPickup_NotBetter_LeavesItOnFloor()
    {
        var player = new PlayerActor(maxHealth: 20, baseAttack: 3);
        player.Loadout.TryEquip(new Weapon("Sword +2", 2));
        var sword = AutoFree(MakeSword("Sword +1", 1));

        var taken = player.TryPickup(sword);

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
