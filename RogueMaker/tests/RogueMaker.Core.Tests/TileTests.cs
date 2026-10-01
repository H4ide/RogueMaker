using RogueMaker.Core.Map;
using RogueMaker.Core.Movement;

namespace RogueMaker.Core.Tests;

public class TileTests
{
    [Theory]
    [InlineData(SurfaceType.Floor, MovementType.Walking, true)]
    [InlineData(SurfaceType.Floor, MovementType.Flying, true)]
    [InlineData(SurfaceType.Exit, MovementType.Walking, true)]
    [InlineData(SurfaceType.Exit, MovementType.Flying, true)]
    [InlineData(SurfaceType.Wall, MovementType.Walking, false)]
    [InlineData(SurfaceType.Wall, MovementType.Flying, false)]
    [InlineData(SurfaceType.Pit, MovementType.Walking, false)]
    [InlineData(SurfaceType.Pit, MovementType.Flying, true)]
    public void Allows_MatchesSurfaceRules(
        SurfaceType surfaceType,
        MovementType movementType,
        bool expected)
    {
        var tile = new Tile(surfaceType);

        Assert.Equal(expected, tile.Allows(movementType));
    }
}
