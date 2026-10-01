using RogueMaker.Core.Map;

namespace RogueMaker.Core.Tests;

public class MapSnapshotTests
{
    [Fact]
    public void CreateSnapshot_KeepsPreviousSurfaceVersionAfterMapMutation()
    {
        var map = new GameMap(2, 1);
        Position position = new(1, 0);

        MapSnapshot snapshot = map.CreateSnapshot();
        map.SetSurface(position, SurfaceType.Wall);

        Assert.Equal(SurfaceType.Floor, snapshot.GetSurface(position).Type);
        Assert.Equal(SurfaceType.Wall, map.GetSurface(position).Type);
    }
}
