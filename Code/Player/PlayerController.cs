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
public partial class PlayerController : Node2D, ICombatant
{
    private GridMover _mover;
    private TurnManager _turnManager;
    private EntityManager _entityManager;

    public Vector2I GridPosition => _mover.GridPosition;
    public bool IsPlayer => true;

    public HealthController Health { get; private set; }
    public int AttackDamage => 5;

    public void Initialize(DungeonGrid gridMap, EntityManager entityManager, TurnManager turnManager, Vector2I startPos)
    {
        _entityManager = entityManager;
        Health = new HealthController(20); // 20 HP for player
        Health.OnDied += Die;
        
        SetupHealthUI();
        
        _mover = new GridMover(this, gridMap, entityManager, startPos);
        _turnManager = turnManager;
        SyncPosition();
        entityManager.RegisterActor(this);
    }

    private void SetupHealthUI()
    {
        var bar = new ProgressBar
        {
            CustomMinimumSize = new Vector2(32, 6),
            Position = new Vector2(-16, -20), // Above the 32x32 sprite
            MaxValue = Health.MaxHp,
            Value = Health.CurrentHp,
            ShowPercentage = false
        };

        // Red background, Green fill
        var bgStyle = new StyleBoxFlat { BgColor = Colors.DarkRed };
        var fgStyle = new StyleBoxFlat { BgColor = Colors.Green };
        
        bar.AddThemeStyleboxOverride("background", bgStyle);
        bar.AddThemeStyleboxOverride("fill", fgStyle);

        AddChild(bar);

        // Update bar visually when C# events fire
        Health.OnHealthChanged += (current, max) => bar.Value = current;
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

        var direction = Vector2I.Zero;

        if (@event.IsActionPressed("move_up"))
            direction = Vector2I.Up;
        else if (@event.IsActionPressed("move_down"))
            direction = Vector2I.Down;
        else if (@event.IsActionPressed("move_left"))
            direction = Vector2I.Left;
        else if (@event.IsActionPressed("move_right"))
            direction = Vector2I.Right;
        else if (@event.IsActionPressed("move_up_left"))
            direction = new Vector2I(-1, -1);
        else if (@event.IsActionPressed("move_up_right"))
            direction = new Vector2I(1, -1);
        else if (@event.IsActionPressed("move_down_left"))
            direction = new Vector2I(-1, 1);
        else if (@event.IsActionPressed("move_down_right"))
            direction = new Vector2I(1, 1);

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

    public void Die()
    {
        _entityManager.UnregisterActor(this);
        QueueFree();
    }
}
