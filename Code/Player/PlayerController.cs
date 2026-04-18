using Godot;
using RogueLike.Code.Grid;
using RogueLike.Code.Entities;
using RogueLike.Code.Entities.Combat;
using RogueLike.Code.TurnContext;

using RogueLike.Code.Items;

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

    public override Vector2I GridPosition => _mover.GridPosition;
    public override bool IsPlayer => true;
    public override int AttackDamage => BaseAttackDamage;
    public Inventory Inventory => _inventory;

    public void Initialize(DungeonGrid gridMap, EntityManager entityManager, TurnManager turnManager, ItemManager itemManager, Vector2I startPos)
    {
        InitializeBase(entityManager);
        
        _mover = new GridMover(this, gridMap, entityManager, startPos);
        _turnManager = turnManager;
        _itemManager = itemManager;
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
            // Wait/Rest action
            if (keyEvent.Keycode == Key.Period)
            {
                _turnManager.EndPlayerTurn();
                return;
            }

            // Use item from inventory (keys 1-9)
            if (keyEvent.Keycode >= Key.Key1 && keyEvent.Keycode <= Key.Key9)
            {
                int slot = (int)keyEvent.Keycode - (int)Key.Key1;
                if (_inventory.UseItem(slot, this))
                {
                    _turnManager.EndPlayerTurn();
                }
                return;
            }
        }

        var direction = InputMapper.GetDirection(@event);

        if (direction == Vector2I.Zero)
            return;

        if (TryMove(direction))
        {
            _turnManager.EndPlayerTurn();
        }
    }

    private void SyncPosition()
    {
        if (_mover != null)
            Position = _mover.WorldPosition;
    }
}
