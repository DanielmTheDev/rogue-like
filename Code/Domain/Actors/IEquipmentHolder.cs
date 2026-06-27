using RogueLike.Domain.Equipment;

namespace RogueLike.Domain.Actors;

/// <summary>
/// An actor that owns a <see cref="Loadout"/> of equipped gear. Lets floor items
/// equip onto the actor without depending on Godot.
/// </summary>
public interface IEquipmentHolder
{
    Loadout Loadout { get; }
}
