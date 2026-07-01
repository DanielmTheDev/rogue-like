using Godot;
using RogueLike.Domain.Common;
using RogueLike.Domain.Grid;
using RogueLike.Domain.Grid.FOV;
using RogueLike.Code.View.Entities;
using RogueLike.Domain.Actors;
using RogueLike.Domain.Combat;
using RogueLike.Domain.Equipment;
using RogueLike.Domain.Flow;
using RogueLike.Domain.Items;
using System.Linq;

namespace RogueLike.Code.View.Player;

/// <summary>
/// Handles player input and grid-based movement.
/// Delegates movement logic to GridMover (pure C# / testable).
/// </summary>
public partial class PlayerController : ActorController, IEquipmentHolder
{
    private GridMover _mover;
    private TurnManager _turnManager;
    private FloorItems _floorItems;
    private Inventory _inventory;
    private FovMap _fovMap;
    private NodeRegistry _nodeRegistry;
    private Main _main; // Reference to Main to trigger level changes

    public bool IsDead { get; private set; } = false;

    private int _turnsSinceLastHeal = 0;
    private const int TurnsPerHeal = 3;

    public override GridPos GridPosition => _mover.GridPosition;
    public override bool IsPlayer => true;
    public override int AttackDamage => BaseAttackDamage + Loadout.DamageBonus;
    public Inventory Inventory => _inventory;
    public Loadout Loadout { get; } = new();
    public ExperienceSystem Experience { get; private set; }

    [Export] public AudioStreamPlayer StepSound { get; set; }

    public void Initialize(Main main, TurnManager turnManager, FloorItems floorItems)
    {
        _main = main;
        _turnManager = turnManager;
        _floorItems = floorItems;

        _inventory = new Inventory(maxSlots: 10);
        Experience = new ExperienceSystem();
        Experience.OnLevelUp += HandleLevelUp;
    }

    public void PlaceOnLevel(DungeonGrid gridMap, ActorRegistry actorRegistry, FovMap fovMap, NodeRegistry nodeRegistry, GridPos startPos)
    {
        InitializeBase(actorRegistry);
        _mover = new GridMover(this, gridMap, actorRegistry, startPos);
        _fovMap = fovMap;
        _nodeRegistry = nodeRegistry;
        SyncPosition();
        actorRegistry.RegisterActor(this);
    }

    /// <summary>
    /// Attempts to move the player one tile in the given direction.
    /// Returns true if an action (move or combat) was successfully consumed.
    /// </summary>
    public bool TryMove(Direction direction)
    {
        if (_mover == null || _turnManager == null)
            return false;

        var target = GridPosition.Step(direction);

        // Check for stairs
        var nodeAtTarget = _nodeRegistry.GetNodeAt(target);
        if (nodeAtTarget is World.StairsController)
        {
            _main.DescendLevel();
            return false; // Don't consume a turn, the level change handles it
        }

        // If an actor is there, bump attack!
        if (_actorRegistry.IsOccupied(target))
        {
            var targetActor = _actorRegistry.GetActorAt(target);
            if (targetActor is ICombatant targetCombatant)
            {
                // TRANSITIONAL (DDD Phase 3): cast needed because TryAttack is a default interface
                // method; disappears when the pure Actor aggregate exposes Attack() directly.
                ((ICombatant)this).TryAttack(targetCombatant);
                return true; // Successfully consumed turn with an attack
            }
        }

        // Otherwise attempt standard movement
        if (!_mover.TryMove(direction))
            return false;

        SyncPosition();
        StepSound?.Play();

        // CHECK FOR ITEMS (auto-pickup). Items are GridPos-native — no conversion needed.
        _floorItems?.CheckForPickup(GridPosition, this, _inventory);

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

    /// <summary>
    /// The player gains XP when its attack kills a combatant (rich-domain kill reaction;
    /// see <see cref="ICombatant.OnKilled"/>).
    /// </summary>
    // TRANSITIONAL (DDD Phase 3): this XP rule lives on the Godot controller; moves onto the
    // pure Player aggregate (which will own ExperienceTrack) when the controller becomes a View.
    public void OnKilled(ICombatant victim)
    {
        Experience.AddXP(victim.XpReward);
        GameLog.Instance.Log($"[color=yellow]You gained {victim.XpReward} XP![/color]");
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
        var input = InputMapper.GetDirection(@event);
        if (input == Vector2I.Zero) return;

        // Convert the raw input vector to the domain Direction VO at the view edge.
        var direction = Direction.FromDelta(input.X, input.Y);

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
            Position = _mover.GridPosition.ToWorldCenter(_mover.Grid.TileSize);
    }

    /// <summary>
    /// Wait repeatedly until health is full or interrupted.
    /// </summary>
    private void WaitUntilFullHealth()
    {
        var maxTurns = 200; // Safety break
        var turnsWaited = 0;

        while (Health.Current < Health.Max && turnsWaited < maxTurns)
        {
            var hpBefore = Health.Current;

            // Perform one wait turn
            ProcessTurnAction(logHeal: false);
            turnsWaited++;

            // STOP CONDITIONS

            // 1. If we took damage, stop immediately
            if (Health.Current < hpBefore)
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

        if (Health.Current >= Health.Max)
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
    private void ShiftMove(Direction direction)
    {
        // Calculate orthogonal directions for path detection
        var ortho1 = Direction.FromDelta(-direction.Dy, direction.Dx);
        var ortho2 = Direction.FromDelta(direction.Dy, -direction.Dx);

        // Record initial walkability of side-tiles
        var side1WasWalkable = _mover.Grid.IsWalkable(GridPosition.Step(ortho1));
        var side2WasWalkable = _mover.Grid.IsWalkable(GridPosition.Step(ortho2));

        while (true)
        {
            if (ShouldShiftMoveStop(direction, ortho1, ortho2, side1WasWalkable, side2WasWalkable))
            {
                break;
            }
        }
    }

    private bool ShouldShiftMoveStop(Direction direction, Direction ortho1, Direction ortho2, bool side1WasWalkable, bool side2WasWalkable)
    {
        var nextPos = GridPosition.Step(direction);

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

    private bool IsBlocked(GridPos position)
    {
        if (_mover.Grid.GetCell(position) == CellType.Wall)
        {
            GameLog.Instance.Log("[color=gray]You stop at a wall.[/color]");
            return true;
        }

        if (_actorRegistry.IsOccupied(position))
        {
            GameLog.Instance.Log("[color=gray]You stop near an entity.[/color]");
            return true;
        }
        return false;
    }

    private bool HasPathChanged(Direction ortho1, Direction ortho2, bool side1WasWalkable, bool side2WasWalkable)
    {
        var side1IsWalkable = _mover.Grid.IsWalkable(GridPosition.Step(ortho1));
        var side2IsWalkable = _mover.Grid.IsWalkable(GridPosition.Step(ortho2));
        return side1IsWalkable != side1WasWalkable || side2IsWalkable != side2WasWalkable;
    }

    /// <summary>
    /// Checks if any non-player actor is visible in the current FOV.
    /// </summary>
    private bool IsEnemyVisible()
    {
        return _actorRegistry.AllActors
            .Where(actor => !actor.IsPlayer)
            .Any(actor => _fovMap.GetVisibility(actor.GridPosition) == VisibilityState.Visible);
    }

    private void HandleLevelUp(int newLevel)
    {
        // Increase stats
        BaseAttackDamage++;
        IncreaseMaxHp(5); // Heal to full on level up as a bonus

        GameLog.Instance.Log($"[color=purple]You reached Level {newLevel}![/color]");
        GameLog.Instance.Log("[color=green]Your Max HP and Attack Damage increase![/color]");
    }

    private void ProcessTurnAction(bool logHeal = true)
    {
        _turnsSinceLastHeal++;
        if (_turnsSinceLastHeal >= TurnsPerHeal)
        {
            Heal(1);
            _turnsSinceLastHeal = 0;
            if (logHeal && Health.Current < Health.Max)
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

        InitializeHealth();

        // Reset systems
        Inventory.Clear();
        Loadout.Clear();
        Experience = new ExperienceSystem();
        Experience.OnLevelUp += HandleLevelUp;
    }
}
