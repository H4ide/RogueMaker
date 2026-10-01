using RogueMaker.Core.Creatures;
using RogueMaker.Core.Enemies;
using RogueMaker.Core.Game;
using RogueMaker.Core.Map;
using RogueMaker.Core.Movement;

namespace RogueMaker.Core.Tests;

public class WorldSnapshotTests
{

    [Fact]
    public void CreateSnapshot_RemainsUnchangedAfterWorldMutation()
    {
        GameState state = CreateState();
        Position playerStart = state.Player.Position;
        Enemy enemy = Assert.Single(state.Enemies);
        Position enemyStart = enemy.Position;

        WorldSnapshot snapshot = state.CreateSnapshot();
        EnemySnapshot enemySnapshot = Assert.Single(snapshot.Enemies);

        state.Map.SetSurface(playerStart, SurfaceType.Wall);
        Assert.True(state.TryMoveCreature(state.Player.Id, Direction.Right));
        Assert.True(state.TryAttackCreature(state.Player.Id, Direction.Right, out _));

        Assert.Equal(SurfaceType.Floor, snapshot.Map.GetSurface(playerStart).Type);
        Assert.Equal(playerStart, snapshot.Player.Position);
        Assert.Equal(10, snapshot.Player.Health);
        Assert.Equal(enemy.TypeId, enemySnapshot.TypeId);
        Assert.Equal(enemyStart, enemySnapshot.Position);
        Assert.Equal(3, enemySnapshot.Health);
        Assert.Same(snapshot.Player, snapshot.GetCreatureAt(playerStart));
        Assert.Same(enemySnapshot, snapshot.GetCreature(enemy.Id));
        Assert.Null(snapshot.GetCreatureAt(new Position(1, 0)));
    }

    private static GameState CreateState()
    {
        var enemyType = new EnemyType(
            EnemyTypeId.Bat,
            new CreatureStats(3, 2, MovementType.Walking));

        var state = new GameState(
            new GameMap(3, 2),
            new EnemyCatalog([enemyType]),
            new Position(0, 0),
            new CreatureStats(10, 1, MovementType.Walking));

        state.AddEnemy(new Position(2, 0), EnemyTypeId.Bat);
        return state;
    }
}
