using Godot;
using RogueLike.Domain.Equipment;

namespace RogueLike.Code.View.UI;

/// <summary>
/// HUD label showing the player's currently equipped weapon. Renders from the
/// domain Loadout and refreshes whenever the equipment changes.
/// </summary>
public partial class WeaponUI : Control
{
    [Export] public Label WeaponLabel;

    private Loadout _loadout;

    public void Initialize(Loadout loadout)
    {
        _loadout = loadout;
        _loadout.OnEquipmentChanged += UpdateDisplay;
        UpdateDisplay();
    }

    public override void _ExitTree()
    {
        if (_loadout != null)
            _loadout.OnEquipmentChanged -= UpdateDisplay;
    }

    private void UpdateDisplay()
    {
        if (WeaponLabel == null) return;

        var name = _loadout.EquippedWeapon?.Name ?? "Unarmed";
        WeaponLabel.Text = $"Weapon: {name}";
    }
}
