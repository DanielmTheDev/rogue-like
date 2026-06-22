using Godot;
using RogueLike.Code.Domain.Items;
using RogueLike.Code.Domain.Common;
using RogueLike.Code.Domain.Actors;
using RogueLike.Code.Domain.Flow;

namespace RogueLike.Code.View.Items;

/// <summary>
/// Base Godot node for all floor items.
/// </summary>
public partial class ItemController : Node2D, IItem
{
    [Export] public string ItemName { get; set; } = "Generic Item";
    public string DisplayName => ItemName;
    public GridPos GridPosition { get; private set; }
    public virtual bool IsConsumable => true;

    protected FloorItems _floorItems;

    public virtual void Initialize(FloorItems floorItems, GridPos position)
    {
        _floorItems = floorItems;
        GridPosition = position;
        Position = position.ToWorldCenter(32);
        _floorItems.RegisterItem(this);
    }

    public virtual bool CanPickup(IActor actor)
    {
        return true; // By default, all items can be picked up
    }

    public virtual void OnPickup(IActor actor)
    {
        GameLog.Instance.Log($"You pick up the {DisplayName}.");
        // The visual node removes itself once collected (view-side lifecycle).
        QueueFree();
    }

    public virtual bool Use(IActor actor)
    {
        // Default behavior: items do nothing when used
        GameLog.Instance.Log($"You use the {DisplayName}. Nothing happens.");
        return false;
    }
}
