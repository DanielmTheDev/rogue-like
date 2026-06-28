using RogueLike.Domain.Actors;
using RogueLike.Domain.Equipment;
using RogueLike.Domain.Flow;
using RogueLike.Domain.Items;

namespace RogueLike.Code.View.Items;

/// <summary>
/// A weapon lying on the floor. Unlike consumables it does not enter the
/// inventory: stepping onto it equips it onto the actor's Loadout, but only if
/// it beats the currently equipped weapon (otherwise it stays on the floor).
/// </summary>
public partial class WeaponItem : ItemController
{
    private Weapon? _weapon;

    public override bool IsConsumable => false;

    /// <summary>Sets which weapon this floor item grants. Call before pickup.</summary>
    public void Configure(Weapon weapon)
    {
        _weapon = weapon;
        ItemName = weapon.Name;
    }

    public override bool TryPickup(IActor actor, Inventory inventory)
    {
        if (_weapon is not { } weapon)
            return false;
        if (actor is not IEquipmentHolder holder)
            return false;
        if (!holder.Loadout.TryEquip(weapon))
            return false;

        GameLog.Instance.Log($"[color=yellow]You equip the {weapon.Name}! (+{weapon.DamageBonus} damage)[/color]");
        QueueFree();
        return true;
    }
}
