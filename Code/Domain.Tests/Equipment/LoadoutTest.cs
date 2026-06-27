using Xunit;
using RogueLike.Domain.Equipment;

namespace RogueLike.Domain.Tests.Equipment;

public class LoadoutTest
{
    [Fact]
    public void New_HasNoWeapon_AndZeroBonus()
    {
        var loadout = new Loadout();

        Assert.Null(loadout.EquippedWeapon);
        Assert.Equal(0, loadout.DamageBonus);
    }

    [Fact]
    public void TryEquip_IntoEmptySlot_Equips()
    {
        var loadout = new Loadout();

        var equipped = loadout.TryEquip(new Weapon("Sword +1", 1));

        Assert.True(equipped);
        Assert.Equal("Sword +1", loadout.EquippedWeapon?.Name);
        Assert.Equal(1, loadout.DamageBonus);
    }

    [Fact]
    public void TryEquip_BetterWeapon_Replaces()
    {
        var loadout = new Loadout();
        loadout.TryEquip(new Weapon("Sword +1", 1));

        var equipped = loadout.TryEquip(new Weapon("Sword +2", 2));

        Assert.True(equipped);
        Assert.Equal("Sword +2", loadout.EquippedWeapon?.Name);
        Assert.Equal(2, loadout.DamageBonus);
    }

    [Fact]
    public void TryEquip_EqualWeapon_Rejected()
    {
        var loadout = new Loadout();
        loadout.TryEquip(new Weapon("Sword +2", 2));

        var equipped = loadout.TryEquip(new Weapon("Other +2", 2));

        Assert.False(equipped);
        Assert.Equal("Sword +2", loadout.EquippedWeapon?.Name);
    }

    [Fact]
    public void TryEquip_WorseWeapon_Rejected()
    {
        var loadout = new Loadout();
        loadout.TryEquip(new Weapon("Sword +2", 2));

        var equipped = loadout.TryEquip(new Weapon("Sword +1", 1));

        Assert.False(equipped);
        Assert.Equal(2, loadout.DamageBonus);
    }

    [Fact]
    public void OnEquipmentChanged_FiresOnEquip()
    {
        var loadout = new Loadout();
        var fired = 0;
        loadout.OnEquipmentChanged += () => fired++;

        loadout.TryEquip(new Weapon("Sword +1", 1));

        Assert.Equal(1, fired);
    }

    [Fact]
    public void OnEquipmentChanged_DoesNotFireOnReject()
    {
        var loadout = new Loadout();
        loadout.TryEquip(new Weapon("Sword +2", 2));
        var fired = 0;
        loadout.OnEquipmentChanged += () => fired++;

        loadout.TryEquip(new Weapon("Sword +1", 1));

        Assert.Equal(0, fired);
    }

    [Fact]
    public void Clear_RemovesWeapon_AndResetsBonus()
    {
        var loadout = new Loadout();
        loadout.TryEquip(new Weapon("Sword +2", 2));

        loadout.Clear();

        Assert.Null(loadout.EquippedWeapon);
        Assert.Equal(0, loadout.DamageBonus);
    }

    [Fact]
    public void Clear_FiresEquipmentChanged_WhenWeaponWasEquipped()
    {
        var loadout = new Loadout();
        loadout.TryEquip(new Weapon("Sword +1", 1));
        var fired = 0;
        loadout.OnEquipmentChanged += () => fired++;

        loadout.Clear();

        Assert.Equal(1, fired);
    }

    [Fact]
    public void Clear_WhenEmpty_IsSafe()
    {
        var loadout = new Loadout();

        loadout.Clear();

        Assert.Null(loadout.EquippedWeapon);
        Assert.Equal(0, loadout.DamageBonus);
    }
}
