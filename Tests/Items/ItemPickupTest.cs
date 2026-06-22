using GdUnit4;
using RogueLike.Code.Domain.Common;
using RogueLike.Code.Domain.Items;
using RogueLike.Code.Domain.Actors;
using static GdUnit4.Assertions;

namespace RogueLike.Tests.Items;

[TestSuite]
public class ItemPickupTest
{
    [TestCase]
    public void TryPickup_EmptyInventory_AddsItem_AndReturnsTrue()
    {
        var item = new MockItem();
        var inventory = new Inventory(maxSlots: 5);

        var picked = ((IItem)item).TryPickup(new MockActor(), inventory);

        AssertBool(picked).IsTrue();
        AssertInt(inventory.Count).IsEqual(1);
        AssertBool(item.OnPickupCalled).IsTrue();
    }

    [TestCase]
    public void TryPickup_FullInventory_ReturnsFalse_AndDoesNotPickup()
    {
        var inventory = new Inventory(maxSlots: 1);
        inventory.AddItem(new MockItem());
        var item = new MockItem();

        var picked = ((IItem)item).TryPickup(new MockActor(), inventory);

        AssertBool(picked).IsFalse();
        AssertInt(inventory.Count).IsEqual(1);
        AssertBool(item.OnPickupCalled).IsFalse();
    }

    [TestCase]
    public void TryPickup_CannotPickup_ReturnsFalse()
    {
        var item = new MockItem { Pickable = false };
        var inventory = new Inventory(maxSlots: 5);

        var picked = ((IItem)item).TryPickup(new MockActor(), inventory);

        AssertBool(picked).IsFalse();
        AssertInt(inventory.Count).IsEqual(0);
    }

    [TestCase]
    public void TryPickup_NullInventory_ReturnsFalse()
    {
        var item = new MockItem();

        AssertBool(((IItem)item).TryPickup(new MockActor(), null)).IsFalse();
    }

    [TestCase]
    public void CheckForPickup_ItemAtPosition_PicksUpAndUnregisters()
    {
        var manager = new FloorItems();
        var pos = new GridPos(3, 4);
        var item = new MockItem { GridPosition = pos };
        manager.RegisterItem(item);
        var inventory = new Inventory(maxSlots: 5);

        manager.CheckForPickup(pos, new MockActor(), inventory);

        AssertInt(inventory.Count).IsEqual(1);
        AssertBool(item.OnPickupCalled).IsTrue();
        AssertInt(manager.AllItems.Count).IsEqual(0);
    }

    [TestCase]
    public void CheckForPickup_NoItemAtPosition_LeavesItemOnFloor()
    {
        var manager = new FloorItems();
        var item = new MockItem { GridPosition = new GridPos(3, 4) };
        manager.RegisterItem(item);
        var inventory = new Inventory(maxSlots: 5);

        manager.CheckForPickup(new GridPos(0, 0), new MockActor(), inventory);

        AssertInt(inventory.Count).IsEqual(0);
        AssertInt(manager.AllItems.Count).IsEqual(1);
    }

    private class MockItem : IItem
    {
        public string DisplayName { get; set; } = "MockItem";
        public GridPos GridPosition { get; set; }
        public bool IsConsumable { get; set; } = true;
        public bool Pickable { get; set; } = true;
        public bool OnPickupCalled { get; private set; }

        public bool CanPickup(IActor actor) => Pickable;
        public void OnPickup(IActor actor) => OnPickupCalled = true;
        public bool Use(IActor actor) => true;
    }

    private class MockActor : IActor
    {
        public GridPos GridPosition { get; set; }
        public bool IsPlayer { get; set; }
    }
}
