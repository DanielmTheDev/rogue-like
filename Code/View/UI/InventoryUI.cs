using Godot;
using RogueLike.Code.Domain.Items;
using RogueLike.Code.Player;

namespace RogueLike.Code.View.UI;

/// <summary>
/// Displays the player's inventory as a simple text list.
/// </summary>
public partial class InventoryUI : Control
{
    [Export] public RichTextLabel InventoryLabel;

    private Inventory _inventory;

    public void Initialize(Inventory inventory)
    {
        _inventory = inventory;
        _inventory.OnInventoryChanged += UpdateDisplay;
        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        if (InventoryLabel == null || _inventory == null)
            return;

        var text = $"[b]Inventory ({_inventory.Count}/{_inventory.MaxSlots})[/b]\n";

        var grouped = _inventory.GetGroupedItems();
        var displaySlot = 1;

        foreach (var (itemName, count, _) in grouped)
        {
            if (count > 1)
            {
                text += $"[color=white]{displaySlot}.[/color] {itemName} [color=yellow]x{count}[/color]\n";
            }
            else
            {
                text += $"[color=white]{displaySlot}.[/color] {itemName}\n";
            }
            displaySlot++;
        }

        // Show a few empty slots if inventory is not full
        if (_inventory.Count < _inventory.MaxSlots)
        {
            text += $"[color=gray]{displaySlot}. <empty>[/color]\n";
        }

        InventoryLabel.Text = text;
    }

    public override void _ExitTree()
    {
        if (_inventory != null)
        {
            _inventory.OnInventoryChanged -= UpdateDisplay;
        }
    }
}
