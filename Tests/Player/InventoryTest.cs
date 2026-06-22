using GdUnit4;
using Godot;
using RogueLike.Code.Domain.Common;
using RogueLike.Code.Domain.Items;
using RogueLike.Code.Player;
using RogueLike.Code.Items;
using RogueLike.Code.Entities;
using RogueLike.Code.Domain.Actors;
using static GdUnit4.Assertions;

namespace RogueLike.Tests.Player;

[TestSuite]
public class InventoryTest
{
    private class MockItem : IItem
    {
        public string DisplayName { get; set; } = "MockItem";
        public Vector2I GridPosition { get; set; }
        public bool IsConsumable { get; set; } = true;
        public bool UseWasCalled { get; private set; } = false;

        public bool CanPickup(IActor actor) => true;
        public void OnPickup(IActor actor) { }

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

    [TestCase]
    public void NewInventory_IsEmpty()
    {
        var inventory = new Inventory(maxSlots: 5);

        AssertInt(inventory.Count).IsEqual(0);
        AssertBool(inventory.IsFull).IsFalse();
    }

    [TestCase]
    public void AddItem_IncreasesCount()
    {
        var inventory = new Inventory(maxSlots: 5);
        var item = new MockItem();

        var added = inventory.AddItem(item);

        AssertBool(added).IsTrue();
        AssertInt(inventory.Count).IsEqual(1);
    }

    [TestCase]
    public void AddItem_WhenFull_ReturnsFalse()
    {
        var inventory = new Inventory(maxSlots: 2);
        inventory.AddItem(new MockItem());
        inventory.AddItem(new MockItem());

        var added = inventory.AddItem(new MockItem());

        AssertBool(added).IsFalse();
        AssertInt(inventory.Count).IsEqual(2);
        AssertBool(inventory.IsFull).IsTrue();
    }

    [TestCase]
    public void UseItem_CallsItemUse()
    {
        var inventory = new Inventory();
        var item = new MockItem();
        inventory.AddItem(item);
        var actor = new MockActor();

        var used = inventory.UseItem(0, actor);

        AssertBool(used).IsTrue();
        AssertBool(item.UseWasCalled).IsTrue();
    }

    [TestCase]
    public void UseItem_ConsumableItem_RemovedFromInventory()
    {
        var inventory = new Inventory();
        var item = new MockItem { IsConsumable = true };
        inventory.AddItem(item);
        var actor = new MockActor();

        inventory.UseItem(0, actor);

        AssertInt(inventory.Count).IsEqual(0);
    }

    [TestCase]
    public void UseItem_NonConsumableItem_RemainsInInventory()
    {
        var inventory = new Inventory();
        var item = new MockItem { IsConsumable = false };
        inventory.AddItem(item);
        var actor = new MockActor();

        inventory.UseItem(0, actor);

        AssertInt(inventory.Count).IsEqual(1);
    }

    [TestCase]
    public void UseItem_InvalidIndex_ReturnsFalse()
    {
        var inventory = new Inventory();
        var actor = new MockActor();

        var used = inventory.UseItem(5, actor);

        AssertBool(used).IsFalse();
    }

    [TestCase]
    public void GetItem_ReturnsCorrectItem()
    {
        var inventory = new Inventory();
        var item1 = new MockItem { DisplayName = "Item1" };
        var item2 = new MockItem { DisplayName = "Item2" };
        inventory.AddItem(item1);
        inventory.AddItem(item2);

        var retrieved = inventory.GetItem(1);

        AssertString(retrieved.DisplayName).IsEqual("Item2");
    }

    [TestCase]
    public void OnInventoryChanged_FiresWhenItemAdded()
    {
        var inventory = new Inventory();
        var eventFired = false;
        inventory.OnInventoryChanged += () => eventFired = true;

        inventory.AddItem(new MockItem());

        AssertBool(eventFired).IsTrue();
    }
}
