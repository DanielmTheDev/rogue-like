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
        if (@event.IsEcho() || !@event.IsPressed())
            return;

        if (_turnManager.CurrentState != TurnState.Player)
            return;

        if (@event is InputEventKey keyEvent)
        {
            bool isShiftPressed = Input.IsKeyPressed(Key.Shift);

            // Shift + Period: Wait until health is full
            if (keyEvent.Keycode == Key.Period && isShiftPressed)
            {
                WaitUntilFullHealth();
                return;
            }

            // Period: Wait/Rest action (passive heal)
            if (keyEvent.Keycode == Key.Period)
            {
                Health.Heal(1);
                _turnManager.EndPlayerTurn();
                return;
            }

            // Use item from inventory (keys 1-9)
            if (keyEvent.Keycode >= Key.Key1 && keyEvent.Keycode <= Key.Key9)
            {
                int groupSlot = (int)keyEvent.Keycode - (int)Key.Key1;
                if (_inventory.UseItemByGroup(groupSlot, this))
                {
                    _turnManager.EndPlayerTurn();
                }
                return;
            }
        }

        var direction = InputMapper.GetDirection(@event);

        if (direction == Vector2I.Zero)
            return;

        bool isShift = Input.IsKeyPressed(Key.Shift);

        // Shift + Direction: Auto-move until seeing enemy or hitting wall/corner
        if (isShift)
        {
            ShiftMove(direction);
        }
        else if (TryMove(direction))
        {
            Health.Heal(1); // Passive healing per action
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
        // Calculate orthogonal directions to our movement
        // North (0, -1) -> East (1, 0) and West (-1, 0)
        // East (1, 0) -> South (0, 1) and North (0, -1)
        Vector2I ortho1 = new Vector2I(-direction.Y, direction.X);
        Vector2I ortho2 = new Vector2I(direction.Y, -direction.X);

        // Record initial walkability of side-tiles
        bool side1WasWalkable = _mover.Grid.IsWalkable(GridPosition + ortho1);
        bool side2WasWalkable = _mover.Grid.IsWalkable(GridPosition + ortho2);

        while (true)
        {
            var nextPos = GridPosition + direction;
            
            // 1. Check if next position is a wall
            if (_mover.Grid.GetCell(nextPos) == CellType.Wall)
            {
                GameLog.Instance.Log("[color=gray]You stop at a wall.[/color]");
                break;
            }

            // 2. Check if next position is occupied by an entity
            if (_entityManager.IsOccupied(nextPos))
            {
                GameLog.Instance.Log("[color=gray]You stop near an entity.[/color]");
                break;
            }

            // 3. Try to move
            if (!TryMove(direction))
            {
                break;
            }

            Health.Heal(1);
            _turnManager.EndPlayerTurn();

            // 4. Check if any enemy is now visible
            if (IsEnemyVisible())
            {
                GameLog.Instance.Log("[color=yellow]You spot an enemy ahead![/color]");
                break;
            }

            // 5. PATH DETECTION: Stop if the walkability of tiles to our sides changes.
            // This detects corners, junctions, and room entrances/exits.
            bool side1IsWalkable = _mover.Grid.IsWalkable(GridPosition + ortho1);
            bool side2IsWalkable = _mover.Grid.IsWalkable(GridPosition + ortho2);

            if (side1IsWalkable != side1WasWalkable || side2IsWalkable != side2WasWalkable)
            {
                GameLog.Instance.Log("[color=gray]You stop at a change in the path.[/color]");
                break;
            }
        }
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
