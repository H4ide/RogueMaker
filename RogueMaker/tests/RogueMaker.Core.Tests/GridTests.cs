using RogueMaker.Core.Map;
using RogueMaker.Core.Movement;

namespace RogueMaker.Core.Tests;

public class GridTests
{
    [Theory]
    [InlineData(0, 0)]
    [InlineData(0, 1)]
    [InlineData(1, 0)]
    [InlineData(-1, 5)]
    [InlineData(5, -1)]
    public void Constructor_RejectsNonPositiveDimensions(int width, int height)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Grid<int>(width, height));
    }


    [Fact]
    public void Grid_StoresSurfacesWithDifferentMovementRules()
    {
        var grid = new Grid<Tile>(width: 4, height: 1);

        grid[new Position(0, 0)] = new Tile(SurfaceType.Wall);
        grid[new Position(1, 0)] = new Tile(SurfaceType.Pit);
        grid[new Position(2, 0)] = new Tile(SurfaceType.Floor);

        Assert.False(grid[new Position(0, 0)].Allows(MovementType.Walking));
        Assert.False(grid[new Position(0, 0)].Allows(MovementType.Flying));

        Assert.False(grid[new Position(1, 0)].Allows(MovementType.Walking));
        Assert.True(grid[new Position(1, 0)].Allows(MovementType.Flying));

        Assert.True(grid[new Position(2, 0)].Allows(MovementType.Walking));
        Assert.True(grid[new Position(2, 0)].Allows(MovementType.Flying));
        // We did not set xy coordinate (3,0).Default value for SurfaceTile is Floor, so the last cell should be walkable and flyable.
        Assert.True(grid[new Position(3, 0)].Allows(MovementType.Walking));
        Assert.True(grid[new Position(3, 0)].Allows(MovementType.Flying));
    }
}
