using Xunit;
using RogueLike.Domain.Common;
using RogueLike.Domain.Grid;
using RogueLike.Domain.Grid.Generators;

namespace RogueLike.Domain.Tests.Grid.Generators;

public class BspDungeonGeneratorTest
{
    [Fact]
    public void Generate_FillsGridAndReturnsRooms()
    {
        var grid = new DungeonGrid(50, 50);
        var rooms = BspDungeonGenerator.Generate(grid);

        // Assert we got at least a few rooms
        Assert.True(rooms.Count > 2);

        // Asset the center of the first room is indeed walkable Floor
        var r1 = rooms[0];
        var center = r1.Center;

        Assert.True(grid.IsWalkable(center));

        // Assert the outer absolute boundary is Wall
        Assert.False(grid.IsWalkable(new GridPos(0, 0)));
    }

    [Fact]
    public void Generate_WithSameSeed_ProducesIdenticalGrid()
    {
        var a = new DungeonGrid(50, 50);
        var b = new DungeonGrid(50, 50);

        BspDungeonGenerator.Generate(a, seed: 1234);
        BspDungeonGenerator.Generate(b, seed: 1234);

        Assert.Equal(DumpCells(a), DumpCells(b));
    }

    [Fact]
    public void Generate_WithDifferentSeeds_ProducesDifferentGrids()
    {
        var a = new DungeonGrid(50, 50);
        var b = new DungeonGrid(50, 50);

        BspDungeonGenerator.Generate(a, seed: 1);
        BspDungeonGenerator.Generate(b, seed: 2);

        Assert.NotEqual(DumpCells(a), DumpCells(b));
    }

    private static string DumpCells(DungeonGrid grid)
    {
        var sb = new System.Text.StringBuilder(grid.Width * grid.Height);
        for (var y = 0; y < grid.Height; y++)
            for (var x = 0; x < grid.Width; x++)
                sb.Append((char)('0' + (int)grid.GetCell(new GridPos(x, y))));
        return sb.ToString();
    }
}
