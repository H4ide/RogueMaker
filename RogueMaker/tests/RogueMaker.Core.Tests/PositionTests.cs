using RogueMaker.Core.Map;
using RogueMaker.Core.Movement;

namespace RogueMaker.Core.Tests;

public class PositionTests
{
    [Theory]
    [InlineData(Direction.Up, 3, 2)]
    [InlineData(Direction.Down, 3, 4)]
    [InlineData(Direction.Left, 2, 3)]
    [InlineData(Direction.Right, 4, 3)]
    public void Addition_ReturnsNeighbourInDirection(
        Direction direction,
        int expectedX,
        int expectedY)
    {
        var position = new Position(3, 3);

        Position result = position + direction;

        Assert.Equal(new Position(expectedX, expectedY), result);
    }

    [Fact]
    public void Addition_CanReturnPositionOutsideMap()
    {
        var position = new Position(0, 0);

        Position result = position + Direction.Up;

        Assert.Equal(new Position(0, -1), result);
    }

    [Fact]
    public void Position_CanAddressGameMapCell()
    {
        var map = new GameMap(2, 2);
        var position = new Position(1, 1);

        map.SetSurface(position, SurfaceType.Pit);

        Assert.Equal(SurfaceType.Pit, map.GetSurface(position).Type);
        Assert.False(map.AllowsEntry(position, MovementType.Walking));
        Assert.True(map.AllowsEntry(position, MovementType.Flying));
    }

    [Fact]
    public void Position_AdditionCanAccessGameMapCell()
    {
        var map = new GameMap(2, 2);
        var position = new Position(0, 0);

        map.SetSurface(position + Direction.Right, SurfaceType.Wall);

        Assert.Equal(SurfaceType.Wall, map.GetSurface(new Position(1, 0)).Type);
        Assert.False(map.AllowsEntry(new Position(1, 0), MovementType.Walking));
        Assert.False(map.AllowsEntry(new Position(1, 0), MovementType.Flying));
    }
}
