using Godot;
using RogueLike.Domain.Common;
using RogueLike.Domain.Actors;
using RogueLike.Domain.Grid;

namespace RogueLike.Code.View.Enemies;

/// <summary>
/// Godot node for the ranged Skeleton Archer. Adds its own shoot <see cref="Range"/>; all other
/// plumbing lives in <see cref="EnemyControllerBase"/>.
/// </summary>
public partial class ArcherController : EnemyControllerBase
{
    [Export] public int Range { get; set; } = 5;

    protected override EnemyAIBase CreateAi(DungeonGrid grid, ActorRegistry actorRegistry, GridPos startPos)
        => new ArcherAI(this, grid, actorRegistry, startPos, Range, SightRange);
}
