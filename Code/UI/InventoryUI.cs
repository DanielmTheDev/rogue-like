using Godot;
using RogueLike.Code.Player;

namespace RogueLike.Code.UI;

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
        
        for (int i = 0; i < _inventory.MaxSlots; i++)
        {
            var item = _inventory.GetItem(i);
            if (item != null)
            {
                text += $"[color=white]{i + 1}.[/color] {item.DisplayName}\n";
            }
            else
            {
                text += $"[color=gray]{i + 1}. <empty>[/color]\n";
            }
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
