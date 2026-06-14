using Godot;
using RogueLike.Code.Grid;
using RogueLike.Code.Grid.FOV;
using RogueLike.Code.Entities;
using RogueLike.Code.Entities.Combat;
using RogueLike.Code.TurnContext;
using RogueLike.Code.Items;
using RogueLike.Code.Services;
using RogueLike.Code.Systems;
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
    private Main _main; // Reference to Main to trigger level changes

    public bool IsDead { get; private set; } = false;

    private int _turnsSinceLastHeal = 0;
    private const int TurnsPerHeal = 5;

    public override Vector2I GridPosition => _mover.GridPosition;
    public override bool IsPlayer => true;
    public override int AttackDamage => BaseAttackDamage;
    public Inventory Inventory => _inventory;
    public ExperienceSystem Experience { get; private set; }

    public void Initialize(Main main, TurnManager turnManager, ItemManager itemManager)
    {
        _main = main;
        _turnManager = turnManager;
        _itemManager = itemManager;

        _inventory = new Inventory(maxSlots: 10);
        Experience = new ExperienceSystem();
        Experience.OnLevelUp += HandleLevelUp;
    }

    public void PlaceOnLevel(DungeonGrid gridMap, EntityManager entityManager, FovMap fovMap, Vector2I startPos)
    {
        InitializeBase(entityManager);
        _mover = new GridMover(this, gridMap, entityManager, startPos);
        _fovMap = fovMap;
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

        // Check for stairs
        var nodeAtTarget = _entityManager.GetNodeAt(target);
        if (nodeAtTarget is World.StairsController)
        {
            _main.DescendLevel();
            return false; // Don't consume a turn, the level change handles it
        }

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

    public override void Die()
    {
        GameLog.Instance.LogDeath(DisplayName);
        // Don't unregister or QueueFree the player. Just mark as dead.
        IsDead = true;
        // The visible player node will be colored red or replaced with a corpse later.
        GameLog.Instance.Log("[color=red]You have died. Press [Enter] to restart.[/color]");
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event.IsEcho() || !@event.IsPressed())
            return;

        if (IsDead)
        {
            if (@event.IsActionPressed("restart_game"))
            {
                _main.RestartGame();
            }
            return;
        }

        if (_turnManager.CurrentState != TurnState.Player)
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
            ProcessTurnAction();
        }
        return true;
    }

    private bool HandleItemActions(InputEvent @event)
    {
        if (@event is not InputEventKey keyEvent) return false;
        if (keyEvent.Keycode < Key.Key1 || keyEvent.Keycode > Key.Key9) return false;

        var groupSlot = (int)keyEvent.Keycode - (int)Key.Key1;
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
            ProcessTurnAction();
        }
    }

    private void SyncPosition()
    {
        if (_mover != null)
            Position = _mover.WorldPosition;
    }

    /// <summary>
    /// Wait repeatedly until health is full or interrupted.
    /// </summary>
    private void WaitUntilFullHealth()
    {
        var maxTurns = 200; // Safety break
        var turnsWaited = 0;

        while (Health.CurrentHp < Health.MaxHp && turnsWaited < maxTurns)
        {
            var hpBefore = Health.CurrentHp;

            // Perform one wait turn
            ProcessTurnAction(logHeal: false);
            turnsWaited++;

            // STOP CONDITIONS

            // 1. If we took damage, stop immediately
            if (Health.CurrentHp < hpBefore)
            {
                GameLog.Instance.Log("[color=orange]You stop resting because you took damage![/color]");
                break;
            }

            // 2. If an enemy is visible, stop immediately
            if (IsEnemyVisible())
            {
                GameLog.Instance.Log("[color=yellow]You stop resting because an enemy is nearby![/color]");
                break;
            }
        }

        if (Health.CurrentHp >= Health.MaxHp)
        {
            GameLog.Instance.Log("[color=green]You rest until fully healed.[/color]");
        }
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
        var ortho1 = new Vector2I(-direction.Y, direction.X);
        var ortho2 = new Vector2I(direction.Y, -direction.X);

        // Record initial walkability of side-tiles
        var side1WasWalkable = _mover.Grid.IsWalkable(GridPosition + ortho1);
        var side2WasWalkable = _mover.Grid.IsWalkable(GridPosition + ortho2);

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

        ProcessTurnAction();

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
        var side1IsWalkable = _mover.Grid.IsWalkable(GridPosition + ortho1);
        var side2IsWalkable = _mover.Grid.IsWalkable(GridPosition + ortho2);
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

    private void HandleLevelUp(int newLevel)
    {
        // Increase stats
        BaseAttackDamage++;
        Health.IncreaseMaxHp(5); // Heal to full on level up as a bonus

        GameLog.Instance.Log($"[color=purple]You reached Level {newLevel}![/color]");
        GameLog.Instance.Log("[color=green]Your Max HP and Attack Damage increase![/color]");
    }

    private void ProcessTurnAction(bool logHeal = true)
    {
        _turnsSinceLastHeal++;
        if (_turnsSinceLastHeal >= TurnsPerHeal)
        {
            Health.Heal(1);
            _turnsSinceLastHeal = 0;
            if (logHeal && Health.CurrentHp < Health.MaxHp)
            {
                GameLog.Instance.Log("[color=gray]You feel a little better.[/color]");
            }
        }
        _turnManager.EndPlayerTurn();
    }

    /// <summary>
    /// Resets the player's state to its default values for a new game.
    /// </summary>
    public void Reset()
    {
        IsDead = false;

        // Reset stats to their exported defaults
        var defaultPlayer = (PlayerController)GD.Load<PackedScene>("res://Scenes/Player.tscn").Instantiate();
        BaseAttackDamage = defaultPlayer.BaseAttackDamage;
        BaseHealth = defaultPlayer.BaseHealth;

        Health = new HealthController(BaseHealth);
        Health.OnDied += Die;

        // Reset systems
        Inventory.Clear();
        Experience = new ExperienceSystem();
        Experience.OnLevelUp += HandleLevelUp;

        // Re-link health bar in case it was disconnected
        if (HealthBar != null)
        {
            HealthBar.MaxValue = Health.MaxHp;
            HealthBar.Value = Health.CurrentHp;
            Health.OnHealthChanged += (current, max) => HealthBar.Value = current;
        }
    }
}
