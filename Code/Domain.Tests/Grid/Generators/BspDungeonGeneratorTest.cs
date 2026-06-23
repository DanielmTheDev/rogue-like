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
}
