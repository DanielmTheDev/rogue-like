using Godot;
using RogueLike.Code.Grid;
using RogueLike.Code.Domain.Grid.FOV;
using RogueLike.Code.Player;
using RogueLike.Code.View;

namespace RogueLike.Code.UI;

public partial class MinimapController : Control
{
    private const int CellPx = 2;
    private DungeonGrid _grid;
    private FovMap _fov;
    private PlayerController _player;

    public void Initialize(DungeonGrid grid, FovMap fov, PlayerController player)
    {
        _grid = grid;
        _fov = fov;
        _player = player;
        QueueRedraw();
    }

    public void Refresh() => QueueRedraw();

    public override void _Draw()
    {
        if (_grid == null || _fov == null) return;
        for (var x = 0; x < _grid.Size.X; x++)
        {
            for (var y = 0; y < _grid.Size.Y; y++)
            {
                var coord = new Vector2I(x, y);
                var vis = _fov.GetVisibility(coord.ToGridPos());
                if (vis == VisibilityState.Unexplored) continue;
                var isWall = _grid.GetCell(coord) == CellType.Wall;
                var color = vis == VisibilityState.Visible
                    ? (isWall ? new Color(0.4f, 0.4f, 0.4f) : new Color(0.8f, 0.8f, 0.8f))
                    : new Color(0.3f, 0.3f, 0.3f);
                DrawRect(new Rect2(x * CellPx, y * CellPx, CellPx, CellPx), color);
            }
        }
        if (_player != null)
        {
            var pp = _player.GridPosition;
            DrawRect(new Rect2(pp.X * CellPx, pp.Y * CellPx, CellPx, CellPx), Colors.Yellow);
        }
    }
}
