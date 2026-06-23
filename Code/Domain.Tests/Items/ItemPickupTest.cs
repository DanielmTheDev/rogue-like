using Xunit;
using RogueLike.Domain.Common;
using RogueLike.Domain.Items;
using RogueLike.Domain.Actors;

namespace RogueLike.Domain.Tests.Items;

public class ItemPickupTest
{
    [Fact]
    public void TryPickup_EmptyInventory_AddsItem_AndReturnsTrue()
    {
        var item = new MockItem();
        var inventory = new Inventory(maxSlots: 5);

        var picked = ((IItem)item).TryPickup(new MockActor(), inventory);

        Assert.True(picked);
        Assert.Equal(1, inventory.Count);
        Assert.True(item.OnPickupCalled);
    }

    [Fact]
    public void TryPickup_FullInventory_ReturnsFalse_AndDoesNotPickup()
    {
        var inventory = new Inventory(maxSlots: 1);
        inventory.AddItem(new MockItem());
        var item = new MockItem();

        var picked = ((IItem)item).TryPickup(new MockActor(), inventory);

        Assert.False(picked);
        Assert.Equal(1, inventory.Count);
        Assert.False(item.OnPickupCalled);
    }

    [Fact]
    public void TryPickup_CannotPickup_ReturnsFalse()
    {
        var item = new MockItem { Pickable = false };
        var inventory = new Inventory(maxSlots: 5);

        var picked = ((IItem)item).TryPickup(new MockActor(), inventory);

        Assert.False(picked);
        Assert.Equal(0, inventory.Count);
    }

    [Fact]
    public void TryPickup_NullInventory_ReturnsFalse()
    {
        var item = new MockItem();

        Assert.False(((IItem)item).TryPickup(new MockActor(), null));
    }

    [Fact]
    public void CheckForPickup_ItemAtPosition_PicksUpAndUnregisters()
    {
        var manager = new FloorItems();
        var pos = new GridPos(3, 4);
        var item = new MockItem { GridPosition = pos };
        manager.RegisterItem(item);
        var inventory = new Inventory(maxSlots: 5);

        manager.CheckForPickup(pos, new MockActor(), inventory);

        Assert.Equal(1, inventory.Count);
        Assert.True(item.OnPickupCalled);
        Assert.Empty(manager.AllItems);
    }

    [Fact]
    public void CheckForPickup_NoItemAtPosition_LeavesItemOnFloor()
    {
        var manager = new FloorItems();
        var item = new MockItem { GridPosition = new GridPos(3, 4) };
        manager.RegisterItem(item);
        var inventory = new Inventory(maxSlots: 5);

        manager.CheckForPickup(new GridPos(0, 0), new MockActor(), inventory);

        Assert.Equal(0, inventory.Count);
        Assert.Single(manager.AllItems);
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
