using GdUnit4;
using Godot;
using RogueLike.Code.Domain.Common;
using RogueLike.Code.Entities;
using RogueLike.Code.Items;
using RogueLike.Code.Player;
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

    private class MockItem : IItem
    {
        public string DisplayName { get; set; } = "MockItem";
        public Vector2I GridPosition { get; set; }
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
