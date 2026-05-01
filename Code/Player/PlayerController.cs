using Godot;
using RogueLike.Code.Grid;
using RogueLike.Code.Grid.FOV;
using RogueLike.Code.Entities;
using RogueLike.Code.Entities.Combat;
using RogueLike.Code.TurnContext;
using RogueLike.Code.Items;
using RogueLike.Code.Services;
using System.Linq;

namespace RogueLike.Code.Player;

/// <summary>
/// Handles player input and grid-based movement.
/// Delegates movement logic to GridMover (pure C# / testable).
/// </summary>
public partial class PlayerController : ActorController
{
    private GridMover _mover;
    private TurnManager _turnManager;
    private ItemManager _itemManager;
    private Inventory _inventory;
    private FovMap _fovMap;

    public override Vector2I GridPosition => _mover.GridPosition;
    public override bool IsPlayer => true;
    public override int AttackDamage => BaseAttackDamage;
    public Inventory Inventory => _inventory;

    public void Initialize(DungeonGrid gridMap, EntityManager entityManager, TurnManager turnManager, ItemManager itemManager, FovMap fovMap, Vector2I startPos)
    {
        InitializeBase(entityManager);
        
        _mover = new GridMover(this, gridMap, entityManager, startPos);
        _turnManager = turnManager;
        _itemManager = itemManager;
        _fovMap = fovMap;
        _inventory = new Inventory(maxSlots: 10);
        SyncPosition();
        entityManager.RegisterActor(this);
    }

    /// <summary>
    /// Attempts to move the player one tile in the given direction.
    /// Returns true if an action (move or combat) was successfully consumed.
    /// </summary>
    public bool TryMove(Vector2I direction)
    {
        if (_mover == null || _turnManager == null)
            return false;

        var target = GridPosition + direction;
        
        // If an actor is there, bump attack!
        if (_entityManager.IsOccupied(target))
        {
            var targetActor = _entityManager.GetActorAt(target);
            if (targetActor is ICombatant targetCombatant)
            {
                CombatSystem.ResolveBump(this, targetCombatant);
                return true; // Successfully consumed turn with an attack
            }
        }

        // Otherwise attempt standard movement
        if (!_mover.TryMove(direction))
            return false;

        SyncPosition();
        
        // CHECK FOR ITEMS (auto-pickup)
        _itemManager?.CheckForPickup(GridPosition, this, _inventory);
        
        return true;
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event.IsEcho() || !@event.IsPressed() || _turnManager.CurrentState != TurnState.Player)
            return;

        if (HandleWaitActions(@event)) return;
        if (HandleItemActions(@event)) return;
        HandleMovementActions(@event);
    }

    private bool HandleWaitActions(InputEvent @event)
    {
        if (!@event.IsActionPressed("wait")) return false;
        
        if (Input.IsKeyPressed(Key.Shift))
        {
            WaitUntilFullHealth();
        }
        else
        {
            Health.Heal(1);
            _turnManager.EndPlayerTurn();
        }
        return true;
    }

    private bool HandleItemActions(InputEvent @event)
    {
        if (@event is not InputEventKey keyEvent) return false;
        if (keyEvent.Keycode < Key.Key1 || keyEvent.Keycode > Key.Key9) return false;
        
        int groupSlot = (int)keyEvent.Keycode - (int)Key.Key1;
        if (_inventory.UseItemByGroup(groupSlot, this))
        {
            _turnManager.EndPlayerTurn();
        }
        return true;
    }

    private void HandleMovementActions(InputEvent @event)
    {
        var direction = InputMapper.GetDirection(@event);
        if (direction == Vector2I.Zero) return;

        if (Input.IsKeyPressed(Key.Shift))
        {
            ShiftMove(direction);
        }
        else if (TryMove(direction))
        {
            Health.Heal(1);
            _turnManager.EndPlayerTurn();
        }
    }

    private void SyncPosition()
    {
        if (_mover != null)
            Position = _mover.WorldPosition;
    }

    /// <summary>
    /// Wait repeatedly until health is full.
    /// </summary>
    private void WaitUntilFullHealth()
    {
        while (Health.CurrentHp < Health.MaxHp)
        {
            Health.Heal(1);
            _turnManager.EndPlayerTurn();
            // Enemy turns happen automatically via TurnManager events
        }
        GameLog.Instance.Log("[color=green]You rest until fully healed.[/color]");
    }

    /// <summary>
    /// Auto-move in a direction until:
    /// - An enemy becomes visible
    /// - A wall is hit
    /// - A corner is detected (change in adjacent wall count)
    /// <summary>
    /// Auto-move in a direction until:
    /// - An enemy becomes visible
    /// - A wall is hit
    /// - The "path" changes (orthogonal walkability changes)
    /// </summary>
    private void ShiftMove(Vector2I direction)
    {
        // Calculate orthogonal directions for path detection
        Vector2I ortho1 = new Vector2I(-direction.Y, direction.X);
        Vector2I ortho2 = new Vector2I(direction.Y, -direction.X);

        // Record initial walkability of side-tiles
        bool side1WasWalkable = _mover.Grid.IsWalkable(GridPosition + ortho1);
        bool side2WasWalkable = _mover.Grid.IsWalkable(GridPosition + ortho2);

        while (true)
        {
            if (ShouldShiftMoveStop(direction, ortho1, ortho2, side1WasWalkable, side2WasWalkable))
            {
                break;
            }
        }
    }

    private bool ShouldShiftMoveStop(Vector2I direction, Vector2I ortho1, Vector2I ortho2, bool side1WasWalkable, bool side2WasWalkable)
    {
        var nextPos = GridPosition + direction;

        if (IsBlocked(nextPos)) return true;
        
        // Try to move
        if (!TryMove(direction)) return true;
        
        Health.Heal(1);
        _turnManager.EndPlayerTurn();

        if (IsEnemyVisible())
        {
            GameLog.Instance.Log("[color=yellow]You spot an enemy ahead![/color]");
            return true;
        }

        if (HasPathChanged(ortho1, ortho2, side1WasWalkable, side2WasWalkable))
        {
            GameLog.Instance.Log("[color=gray]You stop at a change in the path.[/color]");
            return true;
        }

        return false;
    }

    private bool IsBlocked(Vector2I position)
    {
        if (_mover.Grid.GetCell(position) == CellType.Wall)
        {
            GameLog.Instance.Log("[color=gray]You stop at a wall.[/color]");
            return true;
        }

        if (_entityManager.IsOccupied(position))
        {
            GameLog.Instance.Log("[color=gray]You stop near an entity.[/color]");
            return true;
        }
        return false;
    }

    private bool HasPathChanged(Vector2I ortho1, Vector2I ortho2, bool side1WasWalkable, bool side2WasWalkable)
    {
        bool side1IsWalkable = _mover.Grid.IsWalkable(GridPosition + ortho1);
        bool side2IsWalkable = _mover.Grid.IsWalkable(GridPosition + ortho2);
        return side1IsWalkable != side1WasWalkable || side2IsWalkable != side2WasWalkable;
    }

    /// <summary>
    /// Checks if any non-player actor is visible in the current FOV.
    /// </summary>
    private bool IsEnemyVisible()
    {
        return _entityManager.AllActors
            .Where(actor => !actor.IsPlayer)
            .Any(actor => _fovMap.GetVisibility(actor.GridPosition) == VisibilityState.Visible);
    }
}
