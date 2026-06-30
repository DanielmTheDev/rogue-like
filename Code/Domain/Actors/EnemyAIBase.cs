using System.Linq;
using RogueLike.Domain.Common;
using RogueLike.Domain.Grid;
using RogueLike.Domain.Grid.FOV;

namespace RogueLike.Domain.Actors;

/// <summary>
/// Shared decision skeleton for enemy AIs: act on a visible player, otherwise pursue the last-known
/// position and forget it on arrival (or when the path is lost). Subclasses supply only what to do
/// while the player is visible (<see cref="ActOnVisible"/>) — melee chase, ranged fire, etc.
/// </summary>
// TRANSITIONAL (DDD Phase 3.3): each entity is still split across a Controller (node) + this AI +
// the GridMover it owns (which holds the position). Target: a pure Enemy : Actor aggregate that owns
// position/health and its own DecideAndAct(TurnContext); this base folds into that aggregate.
public abstract class EnemyAIBase
{
    protected IActor Owner { get; }
    protected DungeonGrid Grid { get; }
    protected ActorRegistry Actors { get; }
    protected GridMover Mover { get; }

    // Last tile the player was seen on. Drives pursuit after line-of-sight breaks; cleared once
    // reached (or the path is lost) so the enemy gives up instead of chasing omnisciently.
    private GridPos? _lastKnownPlayerPos;

    public GridPos GridPosition => Mover.GridPosition;

    protected EnemyAIBase(IActor owner, DungeonGrid grid, ActorRegistry actorRegistry, GridPos startPos)
    {
        Owner = owner;
        Grid = grid;
        Actors = actorRegistry;
        Mover = new GridMover(owner, grid, actorRegistry, startPos);
    }

    /// <summary>
    /// Evaluates game state and makes a single move.
    /// </summary>
    public void TakeTurn(FovMap fovMap)
    {
        var player = Actors.AllActors.FirstOrDefault(a => a.IsPlayer);
        if (player == null) return;

        if (fovMap.GetVisibility(player.GridPosition) == VisibilityState.Visible)
        {
            _lastKnownPlayerPos = player.GridPosition;
            ActOnVisible(player);
            return;
        }

        PursueLastKnown();
    }

    /// <summary>
    /// What to do when the player is currently visible (e.g. chase + melee, or shoot).
    /// </summary>
    protected abstract void ActOnVisible(IActor player);

    /// <summary>
    /// Steps one tile along the path toward <paramref name="destination"/>; forgets the pursuit
    /// target if no path exists. Pure movement — never attacks (callers handle contact themselves).
    /// </summary>
    protected void StepTowards(GridPos destination)
    {
        var path = Grid.FindPath(Mover.GridPosition, destination);
        if (path is not { Count: > 0 })
        {
            _lastKnownPlayerPos = null;
            return;
        }

        Mover.TryMove(Mover.GridPosition.DirectionTo(path[0]));
    }

    private void PursueLastKnown()
    {
        if (_lastKnownPlayerPos is not { } target) return;

        if (Mover.GridPosition == target)
            _lastKnownPlayerPos = null;
        else
            StepTowards(target);
    }
}
