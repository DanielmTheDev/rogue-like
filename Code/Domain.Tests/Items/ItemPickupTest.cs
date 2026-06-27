using Xunit;
using RogueLike.Domain.Common;
using RogueLike.Domain.Items;
using RogueLike.Domain.Actors;

namespace RogueLike.Domain.Tests.Items;

/// <summary>
/// FloorItems registry behaviour: it locates the item under the actor, delegates the
/// pickup decision to that item, and unregisters it only when the item reports it was
/// taken. The pickup orchestration itself lives on the item (ItemController) and is
/// covered by the view tests.
/// </summary>
public class ItemPickupTest
{
    [Fact]
    public void CheckForPickup_ItemTaken_DelegatesAndUnregisters()
    {
        var manager = new FloorItems();
        var pos = new GridPos(3, 4);
        var item = new MockItem { GridPosition = pos, TryPickupResult = true };
        manager.RegisterItem(item);

        manager.CheckForPickup(pos, new MockActor(), new Inventory(maxSlots: 5));

        Assert.True(item.TryPickupCalled);
        Assert.Empty(manager.AllItems);
    }

    [Fact]
    public void CheckForPickup_ItemNotTaken_LeavesItOnFloor()
    {
        var manager = new FloorItems();
        var pos = new GridPos(3, 4);
        var item = new MockItem { GridPosition = pos, TryPickupResult = false };
        manager.RegisterItem(item);

        manager.CheckForPickup(pos, new MockActor(), new Inventory(maxSlots: 5));

        Assert.True(item.TryPickupCalled);
        Assert.Single(manager.AllItems);
    }

    [Fact]
    public void CheckForPickup_NoItemAtPosition_DoesNothing()
    {
        var manager = new FloorItems();
        var item = new MockItem { GridPosition = new GridPos(3, 4) };
        manager.RegisterItem(item);

        manager.CheckForPickup(new GridPos(0, 0), new MockActor(), new Inventory(maxSlots: 5));

        Assert.False(item.TryPickupCalled);
        Assert.Single(manager.AllItems);
    }

    private class MockItem : IItem
    {
        public string DisplayName { get; set; } = "MockItem";
        public GridPos GridPosition { get; set; }
        public bool IsConsumable { get; set; } = true;
        public bool TryPickupResult { get; set; } = true;
        public bool TryPickupCalled { get; private set; }

        public bool CanPickup(IActor actor) => true;
        public void OnPickup(IActor actor) { }
        public bool Use(IActor actor) => true;

        public bool TryPickup(IActor actor, Inventory inventory)
        {
            TryPickupCalled = true;
            return TryPickupResult;
        }
    }

    private class MockActor : IActor
    {
        public GridPos GridPosition { get; set; }
        public bool IsPlayer { get; set; }
    }
}
