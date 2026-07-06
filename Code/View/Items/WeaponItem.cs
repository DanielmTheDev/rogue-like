using RogueLike.Domain.Equipment;
using RogueLike.Domain.Flow;

namespace RogueLike.Code.View.Items;

/// <summary>
/// A weapon lying on the floor. Unlike consumables it does not enter the inventory: it opts into
/// equipping via <see cref="IEquippable"/>, and the picker equips it onto its Loadout only if it beats
/// the currently equipped weapon (otherwise the picker leaves it on the floor).
/// </summary>
public partial class WeaponItem : ItemController, IEquippable
{
    private Weapon? _weapon;

    public override bool IsConsumable => false;

    /// <summary>The weapon this floor item grants (read by the picker to equip it).</summary>
    public Weapon Weapon => _weapon!.Value;

    /// <summary>Sets which weapon this floor item grants. Call before pickup.</summary>
    public void Configure(Weapon weapon)
    {
        _weapon = weapon;
        ItemName = weapon.Name;
    }

    public override bool CanPickup() => _weapon.HasValue;

    public override void OnPickup()
    {
        var weapon = _weapon!.Value;
        GameLog.Instance.Log($"[color=yellow]You equip the {weapon.Name}! (+{weapon.DamageBonus} damage)[/color]");
        QueueFree();
    }
}
