using Xunit;
using RogueLike.Domain.Common;
using RogueLike.Domain.Items;
using RogueLike.Domain.Actors;

namespace RogueLike.Domain.Tests.Items;

/// <summary>
/// FloorItems registry behaviour: it locates the item under the picker, delegates the acquire to the
/// picker (<see cref="IItemPicker.TryPickup"/>), and unregisters it only when the picker reports it was
/// taken. The acquire decision (equip vs store) lives on the Player aggregate (covered by PlayerTest).
/// </summary>
public class ItemPickupTest
{
    [Fact]
    public void CheckForPickup_ItemTaken_DelegatesAndUnregisters()
    {
        var manager = new FloorItems();
        var pos = new GridPos(3, 4);
        var item = new MockItem { GridPosition = pos };
        manager.RegisterItem(item);
        var picker = new MockPicker { Result = true };

        manager.CheckForPickup(pos, picker);

        Assert.Same(item, picker.PickedItem);
        Assert.Empty(manager.AllItems);
    }

    [Fact]
    public void CheckForPickup_ItemNotTaken_LeavesItOnFloor()
    {
        var manager = new FloorItems();
        var pos = new GridPos(3, 4);
        var item = new MockItem { GridPosition = pos };
        manager.RegisterItem(item);
        var picker = new MockPicker { Result = false };

        manager.CheckForPickup(pos, picker);

        Assert.Same(item, picker.PickedItem);
        Assert.Single(manager.AllItems);
    }

    [Fact]
    public void CheckForPickup_NoItemAtPosition_DoesNothing()
    {
        var manager = new FloorItems();
        var item = new MockItem { GridPosition = new GridPos(3, 4) };
        manager.RegisterItem(item);
        var picker = new MockPicker();

        manager.CheckForPickup(new GridPos(0, 0), picker);

        Assert.Null(picker.PickedItem);
        Assert.Single(manager.AllItems);
    }

    private class MockItem : IItem
    {
        public string DisplayName { get; set; } = "MockItem";
        public GridPos GridPosition { get; set; }
        public bool IsConsumable { get; set; } = true;

        public bool CanPickup() => true;
        public void OnPickup() { }
        public bool Use(IActor actor) => true;
    }

    private class MockPicker : IItemPicker
    {
        public bool Result { get; init; } = true;
        public IItem PickedItem { get; private set; }

        public bool TryPickup(IItem item)
        {
            PickedItem = item;
            return Result;
        }
    }
}
