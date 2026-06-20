using System.Linq;
using Godot;
using RogueLike.Code.Domain.Common;
using RogueLike.Code.Entities;
using RogueLike.Code.Entities.Combat;
using RogueLike.Code.Grid;
using RogueLike.Code.Grid.FOV;
using RogueLike.Code.Player;

namespace RogueLike.Code.Enemies;

/// <summary>
/// Pure C# AI for the Skeleton Archer.
/// Behavior: If player is in LOS and within range → shoot.
///           If player is visible but out of range → walk closer.
///           Otherwise → idle.
/// </summary>
// TRANSITIONAL (DDD Phase 3.3): the entity is split across ArcherController (node) + this AI +
// the GridMover it owns (which actually holds the archer's position). Target: a pure
// Archer : Enemy : Actor aggregate that owns position/health and its own DecideAndAct(TurnContext);
// ArcherController demotes to a pure View. This separate AI object then disappears.
public class ArcherAI
{
    private readonly IActor _owner;
    private readonly DungeonGrid _grid;
    private readonly EntityManager _entityManager;
    private readonly GridMover _mover;
    private readonly int _range;

    public Vector2I GridPosition => _mover.GridPosition;

    public ArcherAI(IActor owner, DungeonGrid grid, EntityManager entityManager, Vector2I startPos, int range = 5)
    {
        _owner = owner;
        _grid = grid;
        _entityManager = entityManager;
        _mover = new GridMover(owner, grid, entityManager, startPos);
        _range = range;
    }

    public void TakeTurn(FovMap fovMap)
    {
        var player = _entityManager.AllActors.FirstOrDefault(a => a.IsPlayer);
        if (player == null || !IsVisible(player, fovMap))
            return;

        // Player is visible: shoot if in clear range, otherwise close the distance.
        if (CanShoot(player))
            Shoot(player);
        else
            ChasePlayer(player);
    }

    private static bool IsVisible(IActor player, FovMap fovMap)
        => fovMap.GetVisibility(player.GridPosition) == VisibilityState.Visible;

    private bool CanShoot(IActor player)
    {
        var ownerPos = new GridPos(_owner.GridPosition.X, _owner.GridPosition.Y);
        var playerPos = new GridPos(player.GridPosition.X, player.GridPosition.Y);
        return _grid.HasClearLine(ownerPos, playerPos) && ownerPos.ManhattanTo(playerPos) <= _range;
    }

    private void Shoot(IActor player)
    {
        if (_owner is ICombatant attacker && player is ICombatant defender)
            attacker.TryAttack(defender);
    }

    private void ChasePlayer(IActor player)
    {
        var path = _grid.FindPath(_owner.GridPosition, player.GridPosition);
        if (path != null && path.Count > 0)
        {
            var direction = path[0] - _owner.GridPosition;
            _mover.TryMove(direction);
        }
    }
}
