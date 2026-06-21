using Godot;
using RogueLike.Code.Domain.Common;

namespace RogueLike.Code.Grid.FOV;

/// <summary>
/// A TileMapLayer responsible strictly for drawing shadows over the view.
/// </summary>
public partial class FovTileMap : TileMapLayer
{
    private const int SourceUnexplored = 0;
    private const int SourceExplored = 1;

    public void Initialize()
    {
        var tileSet = new TileSet();
        tileSet.TileSize = new Vector2I(32, 32);

        // Map Unexplored
        var texUnexplored = GD.Load<Texture2D>("res://Assets/Tiles/black_tile.png");
        var sourceUnexplored = new TileSetAtlasSource { Texture = texUnexplored, TextureRegionSize = new Vector2I(32, 32) };
        sourceUnexplored.CreateTile(new Vector2I(0, 0));
        tileSet.AddSource(sourceUnexplored, SourceUnexplored);

        // Map Explored
        var texExplored = GD.Load<Texture2D>("res://Assets/Tiles/shadow_tile.png");
        var sourceExplored = new TileSetAtlasSource { Texture = texExplored, TextureRegionSize = new Vector2I(32, 32) };
        sourceExplored.CreateTile(new Vector2I(0, 0));
        tileSet.AddSource(sourceExplored, SourceExplored);

        TileSet = tileSet;
        ZIndex = 10;
    }

    public void Render(FovMap map)
    {
        Clear();
        for (var x = 0; x < map.Width; x++)
        {
            for (var y = 0; y < map.Height; y++)
            {
                var state = map.GetVisibility(new GridPos(x, y));
                if (state == VisibilityState.Unexplored)
                {
                    SetCell(new Vector2I(x, y), SourceUnexplored, new Vector2I(0, 0));
                }
                else if (state == VisibilityState.Explored)
                {
                    SetCell(new Vector2I(x, y), SourceExplored, new Vector2I(0, 0));
                }
                // If Visible, we set nothing (it's transparent)
            }
        }
    }
}
