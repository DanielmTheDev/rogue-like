using Godot;
using RogueLike.Code.Grid;
using RogueLike.Code.Entities;
using RogueLike.Code.Entities.Combat;
using RogueLike.Code.TurnContext;

namespace RogueLike.Code.Player;

/// <summary>
/// Handles player input and grid-based movement.
/// Delegates movement logic to GridMover (pure C# / testable).
/// </summary>
public partial class PlayerController : ActorController
{
    private GridMover _mover;
    private TurnManager _turnManager;

    public override Vector2I GridPosition => _mover.GridPosition;
    public override bool IsPlayer => true;
    public override int AttackDamage => 5;

    public void Initialize(DungeonGrid gridMap, EntityManager entityManager, TurnManager turnManager, Vector2I startPos)
    {
        InitializeBase(entityManager, 20); // 20 HP for player
        
        _mover = new GridMover(this, gridMap, entityManager, startPos);
        _turnManager = turnManager;
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
        return true;
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event.IsEcho() || !@event.IsPressed())
            return;

        if (_turnManager.CurrentState != TurnState.Player)
            return;

        if (@event is InputEventKey keyEvent && keyEvent.Keycode == Key.Period)
        {
            Health.Heal(1);
            _turnManager.EndPlayerTurn();
            return;
        }

        var direction = InputMapper.GetDirection(@event);

        if (direction == Vector2I.Zero)
            return;

        if (TryMove(direction))
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
}
