using GdUnit4;
using RogueLike.Code.Domain.Common;
using RogueLike.Code.Domain.Grid;
using RogueLike.Code.Domain.Grid.Generators;
using static GdUnit4.Assertions;

namespace RogueLike.Tests.Grid.Generators;

[TestSuite]
public class BspDungeonGeneratorTest
{
    [TestCase]
    public void Generate_FillsGridAndReturnsRooms()
    {
        var grid = new DungeonGrid(50, 50);
        var rooms = BspDungeonGenerator.Generate(grid);

        // Assert we got at least a few rooms
        AssertBool(rooms.Count > 2).IsTrue();

        // Asset the center of the first room is indeed walkable Floor
        var r1 = rooms[0];
        var center = r1.Center;

        AssertBool(grid.IsWalkable(center)).IsTrue();

        // Assert the outer absolute boundary is Wall
        AssertBool(grid.IsWalkable(new GridPos(0, 0))).IsFalse();
    }
}
