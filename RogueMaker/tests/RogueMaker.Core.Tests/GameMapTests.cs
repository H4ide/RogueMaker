using RogueMaker.Core.Map;
using RogueMaker.Core.Movement;

namespace RogueMaker.Core.Tests;

public class GameMapTests
{
    [Fact]
    public void SetSurface_ChangesSelectedCellOnly()
    {
        var map = new GameMap(2, 1);
        map.SetSurface(new Position(1, 0), SurfaceType.Wall);

        Assert.Equal(SurfaceType.Floor, map.GetSurface(new Position(0, 0)).Type);
        Assert.Equal(SurfaceType.Wall, map.GetSurface(new Position(1, 0)).Type);
    }

    [Theory]
    [InlineData(SurfaceType.Floor, MovementType.Walking, true)]
    [InlineData(SurfaceType.Floor, MovementType.Flying, true)]
    [InlineData(SurfaceType.Exit, MovementType.Walking, true)]
    [InlineData(SurfaceType.Exit, MovementType.Flying, true)]
    [InlineData(SurfaceType.Wall, MovementType.Walking, false)]
    [InlineData(SurfaceType.Wall, MovementType.Flying, false)]
    [InlineData(SurfaceType.Pit, MovementType.Walking, false)]
    [InlineData(SurfaceType.Pit, MovementType.Flying, true)]
    public void MovementQueries_FollowSurfaceRules(
        SurfaceType type,
        MovementType movementType,
        bool expected)
    {
        var map = new GameMap(1, 1);
        var position = new Position(0, 0);
        map.SetSurface(position, type);

        Assert.Equal(expected, map.AllowsEntry(position, movementType));
    }

    [Theory]
    [InlineData(-1, 0)]
    [InlineData(0, 1)]
    public void MovementQueries_ReturnFalseOutsideMap(int x, int y)
    {
        var map = new GameMap(1, 1);
        var position = new Position(x, y);

        Assert.False(map.AllowsEntry(position, MovementType.Walking));
        Assert.False(map.AllowsEntry(position, MovementType.Flying));
    }
}
