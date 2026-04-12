using Godot;

namespace RogueLike.Code.Grid.Generators;

/// <summary>
/// A simple map generator that creates an enclosed arena with random 3x3 pillars.
/// </summary>
public static class PillarArenaGenerator
{
    public static void Generate(DungeonGrid grid)
    {
        // First, ensure everything is Floor.
        // Although DungeonGrid defaults to Floor, it's good practice for a 
        // generator to assume a blank slate before carving.
        for (var x = 0; x < grid.Size.X; x++)
        {
            for (var y = 0; y < grid.Size.Y; y++)
            {
                grid.SetCell(new Vector2I(x, y), CellType.Floor);
            }
        }

        var bounds = grid.Size;

        // Add boundary walls
        for (var x = 0; x < bounds.X; x++)
        {
            grid.SetCell(new Vector2I(x, 0), CellType.Wall);
            grid.SetCell(new Vector2I(x, bounds.Y - 1), CellType.Wall);
        }
        for (var y = 0; y < bounds.Y; y++)
        {
            grid.SetCell(new Vector2I(0, y), CellType.Wall);
            grid.SetCell(new Vector2I(bounds.X - 1, y), CellType.Wall);
        }

        // Add some random inner walls/pillars
        // We use a fixed seed sequence for predictable testing
        var random = new System.Random(1337);
        var numberOfPillars = (bounds.X * bounds.Y) / 100; // rough density

        for (var i = 0; i < numberOfPillars; i++)
        {
            var px = random.Next(2, bounds.X - 4);
            var py = random.Next(2, bounds.Y - 4);

            // Draw a 3x3 pillar
            for (var dx = 0; dx < 3; dx++)
            {
                for (var dy = 0; dy < 3; dy++)
                {
                    grid.SetCell(new Vector2I(px + dx, py + dy), CellType.Wall);
                }
            }
        }
    }
}
