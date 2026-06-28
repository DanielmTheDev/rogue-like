using GdUnit4;
using RogueLike.Code.View.Items;
using RogueLike.Code.View.Player;
using RogueLike.Domain.Items;
using static GdUnit4.Assertions;

namespace RogueLike.Code.View.Tests;

/// <summary>
/// ItemController's default pickup orchestration: take the item into the actor's
/// inventory, honouring CanPickup, a full inventory, and a missing inventory.
/// (Moved here from the domain when IItem's default TryPickup was removed.)
/// </summary>
[TestSuite]
[RequireGodotRuntime]
public class ItemPickupTest
{
    [TestCase]
    public void TryPickup_EmptyInventory_AddsItem_AndReturnsTrue()
    {
        IItem item = AutoFree(new ItemController());
        var inventory = new Inventory(maxSlots: 5);

        var picked = item.TryPickup(AutoFree(new PlayerController()), inventory);

        AssertBool(picked).IsTrue();
        AssertInt(inventory.Count).IsEqual(1);
    }

    [TestCase]
    public void TryPickup_FullInventory_ReturnsFalse_AndDoesNotPickup()
    {
        var inventory = new Inventory(maxSlots: 1);
        inventory.AddItem(AutoFree(new ItemController()));
        IItem item = AutoFree(new ItemController());

        var picked = item.TryPickup(AutoFree(new PlayerController()), inventory);

        AssertBool(picked).IsFalse();
        AssertInt(inventory.Count).IsEqual(1);
    }

    [TestCase]
    public void TryPickup_NullInventory_ReturnsFalse()
    {
        IItem item = AutoFree(new ItemController());

        var picked = item.TryPickup(AutoFree(new PlayerController()), null);

        AssertBool(picked).IsFalse();
    }
}
