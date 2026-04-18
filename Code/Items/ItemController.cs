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

    protected ItemManager _itemManager;

    public virtual void Initialize(ItemManager itemManager, Vector2I position)
    {
        _itemManager = itemManager;
        GridPosition = position;
        Position = new Vector2(position.X * 32 + 16, position.Y * 32 + 16);
        _itemManager.RegisterItem(this);
    }

    public virtual bool ProcessPickup(IActor actor)
    {
        // Default behavior: just log it and disappear
        GameLog.Instance.Log($"You walk over the {DisplayName}.");
        QueueFree();
        return true;
    }
}
