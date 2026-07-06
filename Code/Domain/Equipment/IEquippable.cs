namespace RogueLike.Domain.Equipment;

/// <summary>
/// A floor item that equips onto the picker's <see cref="Loadout"/> instead of entering the
/// inventory. The picker (the actor that owns the acquire) reads <see cref="Weapon"/> and decides
/// whether to equip it — the item never reaches into the actor's aggregates.
/// </summary>
public interface IEquippable
{
    Weapon Weapon { get; }
}
