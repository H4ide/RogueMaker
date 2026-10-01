using RogueMaker.Core.Creatures;
using RogueMaker.Core.Enemies;
using RogueMaker.Core.Game;
using RogueMaker.Core.Map;
using RogueMaker.Core.Movement;

namespace RogueMaker.Core.Tests;

public class CreatureTests
{
    [Fact]
    public void Player_StartsWithMaxHealthAndPlayerSide()
    {
        var stats = new CreatureStats(10, 2, MovementType.Walking);
        var state = new GameState(
            new GameMap(10, 10),
            new EnemyCatalog([]),
            new Position(2, 3),
            stats);
        Player player = state.Player;

        Assert.Equal(0, player.Id.Value);
        Assert.Equal(new Position(2, 3), player.Position);
        Assert.Equal(stats, player.Stats);
        Assert.Equal(10, player.Health);
        Assert.True(player.IsAlive);
        Assert.Equal(CreatureSide.Player, player.Side);
    }

    [Fact]
    public void GameState_EnemyStartsWithMaxHealthUsesTypeAndEnemySide()
    {
        var enemyType = new EnemyType(
            EnemyTypeId.Bat,
            new CreatureStats(3, 1, MovementType.Flying));
        var state = CreateGameState(enemyType);
        state.AddEnemy(new Position(4, 5), enemyType.TypeId);
        Enemy enemy = Assert.Single(state.Enemies);

        Assert.Same(enemyType, enemy.EnemyType);
        Assert.Equal(enemyType.TypeId, enemy.TypeId);
        Assert.Equal(enemyType.Stats, enemy.Stats);
        Assert.Equal(3, enemy.Health);
        Assert.True(enemy.IsAlive);
        Assert.Equal(CreatureSide.Enemy, enemy.Side);
    }

    [Fact]
    public void GameState_PlayerInitializationRejectsNullStats()
    {
        Assert.Throws<ArgumentNullException>(
            () => new GameState(
                new GameMap(1, 1),
                new EnemyCatalog([]),
                new Position(0, 0),
                null!));
    }

    [Fact]
    public void GameState_RejectsNullEnemyCatalog()
    {
        Assert.Throws<ArgumentNullException>(
            () => new GameState(
                new GameMap(1, 1),
                null!,
                new Position(0, 0),
                new CreatureStats(10, 2, MovementType.Walking)));
    }

    private static GameState CreateGameState(EnemyType enemyType)
        => new(
            new GameMap(10, 10),
            new EnemyCatalog([enemyType]),
            new Position(0, 0),
            new CreatureStats(10, 2, MovementType.Walking));
}
