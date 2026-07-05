using RogueLike.Domain.Common;
using RogueLike.Domain.Actors;
using RogueLike.Domain.Grid;

namespace RogueLike.Code.View.Enemies;

/// <summary>
/// Godot node for the melee Goblin. All shared plumbing lives in <see cref="EnemyControllerBase"/>;
/// this only wires up the concrete <see cref="EnemyAI"/>.
/// </summary>
public partial class EnemyController : EnemyControllerBase
{
    protected override EnemyAIBase CreateAi(DungeonGrid grid, ActorRegistry actorRegistry, GridPos startPos)
        => new EnemyAI(this, grid, actorRegistry, startPos, SightRange);
}
