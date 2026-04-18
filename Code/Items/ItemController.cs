using Godot;
using RogueLike.Code.Entities;
using RogueLike.Code.Services;

namespace RogueLike.Code.Items;

/// <summary>
/// Base Godot node for all floor items.
/// </summary>
public partial class ItemController : Node2D, IItem
{
    [Export] public string ItemName { get; set; } = "Generic Item";
    public string DisplayName => ItemName;
    public Vector2I GridPosition { get; private set; }
    public virtual bool IsConsumable => true;

    protected ItemManager _itemManager;

    public virtual void Initialize(ItemManager itemManager, Vector2I position)
    {
        _itemManager = itemManager;
        GridPosition = position;
        Position = new Vector2(position.X * 32 + 16, position.Y * 32 + 16);
        _itemManager.RegisterItem(this);
    }

    public virtual bool CanPickup(IActor actor)
    {
        return true; // By default, all items can be picked up
    }

    public virtual void OnPickup(IActor actor)
    {
        GameLog.Instance.Log($"You pick up the {DisplayName}.");
    }

    public virtual bool Use(IActor actor)
    {
        // Default behavior: items do nothing when used
        GameLog.Instance.Log($"You use the {DisplayName}. Nothing happens.");
        return false;
    }
}
