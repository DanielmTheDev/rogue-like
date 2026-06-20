using GdUnit4;
using Godot;
using RogueLike.Code.Grid;
using RogueLike.Code.Grid.Generators;
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
        var center = new Vector2I(r1.Position.X + r1.Size.X / 2, r1.Position.Y + r1.Size.Y / 2);

        AssertBool(grid.IsWalkable(center)).IsTrue();

        // Assert the outer absolute boundary is Wall
        AssertBool(grid.IsWalkable(new Vector2I(0, 0))).IsFalse();
    }
}
