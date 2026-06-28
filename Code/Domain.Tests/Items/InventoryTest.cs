using Xunit;
using RogueLike.Domain.Common;
using RogueLike.Domain.Items;
using RogueLike.Domain.Actors;

namespace RogueLike.Domain.Tests.Items;

public class InventoryTest
{
    private class MockItem : IItem
    {
        public string DisplayName { get; set; } = "MockItem";
        public GridPos GridPosition { get; set; }
        public bool IsConsumable { get; set; } = true;
        public bool UseWasCalled { get; private set; } = false;

        public bool CanPickup(IActor actor) => true;
        public void OnPickup(IActor actor) { }
        public bool TryPickup(IActor actor, Inventory inventory) => false;

        public bool Use(IActor actor)
        {
            UseWasCalled = true;
            return true;
        }
    }

    private class MockActor : IActor
    {
        public GridPos GridPosition { get; set; }
        public bool IsPlayer { get; set; }
    }

    [Fact]
    public void NewInventory_IsEmpty()
    {
        var inventory = new Inventory(maxSlots: 5);

        Assert.Equal(0, inventory.Count);
        Assert.False(inventory.IsFull);
    }

    [Fact]
    public void AddItem_IncreasesCount()
    {
        var inventory = new Inventory(maxSlots: 5);
        var item = new MockItem();

        var added = inventory.AddItem(item);

        Assert.True(added);
        Assert.Equal(1, inventory.Count);
    }

    [Fact]
    public void AddItem_WhenFull_ReturnsFalse()
    {
        var inventory = new Inventory(maxSlots: 2);
        inventory.AddItem(new MockItem());
        inventory.AddItem(new MockItem());

        var added = inventory.AddItem(new MockItem());

        Assert.False(added);
        Assert.Equal(2, inventory.Count);
        Assert.True(inventory.IsFull);
    }

    [Fact]
    public void UseItem_CallsItemUse()
    {
        var inventory = new Inventory();
        var item = new MockItem();
        inventory.AddItem(item);
        var actor = new MockActor();

        var used = inventory.UseItem(0, actor);

        Assert.True(used);
        Assert.True(item.UseWasCalled);
    }

    [Fact]
    public void UseItem_ConsumableItem_RemovedFromInventory()
    {
        var inventory = new Inventory();
        var item = new MockItem { IsConsumable = true };
        inventory.AddItem(item);
        var actor = new MockActor();

        inventory.UseItem(0, actor);

        Assert.Equal(0, inventory.Count);
    }

    [Fact]
    public void UseItem_NonConsumableItem_RemainsInInventory()
    {
        var inventory = new Inventory();
        var item = new MockItem { IsConsumable = false };
        inventory.AddItem(item);
        var actor = new MockActor();

        inventory.UseItem(0, actor);

        Assert.Equal(1, inventory.Count);
    }

    [Fact]
    public void UseItem_InvalidIndex_ReturnsFalse()
    {
        var inventory = new Inventory();
        var actor = new MockActor();

        var used = inventory.UseItem(5, actor);

        Assert.False(used);
    }

    [Fact]
    public void GetItem_ReturnsCorrectItem()
    {
        var inventory = new Inventory();
        var item1 = new MockItem { DisplayName = "Item1" };
        var item2 = new MockItem { DisplayName = "Item2" };
        inventory.AddItem(item1);
        inventory.AddItem(item2);

        var retrieved = inventory.GetItem(1);

        Assert.Equal("Item2", retrieved.DisplayName);
    }

    [Fact]
    public void OnInventoryChanged_FiresWhenItemAdded()
    {
        var inventory = new Inventory();
        var eventFired = false;
        inventory.OnInventoryChanged += () => eventFired = true;

        inventory.AddItem(new MockItem());

        Assert.True(eventFired);
    }
}
