using RogueMaker.Core.AI;
using RogueMaker.Core.Actions;
using RogueMaker.Core.Creatures;
using RogueMaker.Core.Enemies;
using RogueMaker.Core.Game;
using RogueMaker.Core.Map;
using RogueMaker.Core.Movement;
using RogueMaker.Core.Turns;

namespace RogueMaker.Core.Tests;

public class TurnManagerTests
{
    [Fact]
    public void TurnNumber_StartsAtZero()
    {
        GameState state = CreateState(new GameMap(1, 1), new Position(0, 0));

        var manager = new TurnManager(state);

        Assert.Equal(0, manager.TurnNumber);
        Assert.Equal(GameStatus.InProgress, manager.Status);
    }

    [Fact]
    public async Task PerformGameActionAsync_BlockedActionConsumesTurn()
    {
        GameState state = CreateState(new GameMap(1, 1), new Position(0, 0));
        var manager = new TurnManager(state);

        TurnResult result = await manager.PerformGameActionAsync(Direction.Left);

        Assert.Equal(1, manager.TurnNumber);
        Assert.Equal(1, result.TurnNumber);
        Assert.Equal(CreatureActionResult.Blocked, result.PlayerOutcome.Result);
        Assert.Empty(result.EnemyOutcomes);
    }

    [Fact]
    public async Task PerformGameActionAsync_EnemiesSeeWorldAfterPlayerAction()
    {
        GameState state = CreateState(
            new GameMap(2, 2),
            new Position(0, 0),
            new CreatureStats(3, 1, MovementType.Walking),
            new ChaserBrain(),
            new Position(1, 1));
        var manager = new TurnManager(state);

        TurnResult result = await manager.PerformGameActionAsync(Direction.Right);

        CreatureActionOutcome enemyOutcome = Assert.Single(result.EnemyOutcomes);
        Assert.Equal(CreatureActionResult.Moved, result.PlayerOutcome.Result);
        Assert.Equal(CreatureActionResult.Attacked, enemyOutcome.Result);
        Assert.Equal(new Position(1, 0), state.Player.Position);
        Assert.Equal(9, state.Player.Health);
        Assert.Equal(new Position(1, 1), Assert.Single(state.Enemies).Position);
        Assert.Equal(GameStatus.InProgress, result.Status);
        Assert.Equal(GameStatus.InProgress, manager.Status);
    }

    [Fact]
    public async Task PerformGameActionAsync_EnemyStartingOnBlockedSurface_MovesToAllowedSurface()
    {
        var map = new GameMap(4, 1);
        var enemyStart = new Position(0, 0);
        var enemyTarget = new Position(1, 0);
        map.SetSurface(enemyStart, SurfaceType.Wall);
        GameState state = CreateState(
            map,
            new Position(3, 0),
            new CreatureStats(3, 1, MovementType.Walking),
            new ChaserBrain(),
            enemyStart);
        var manager = new TurnManager(state);

        TurnResult result = await manager.PerformGameActionAsync(Direction.Right);

        CreatureActionOutcome enemyOutcome = Assert.Single(result.EnemyOutcomes);
        Assert.Equal(CreatureActionResult.Moved, enemyOutcome.Result);
        Assert.Equal(enemyStart, enemyOutcome.From);
        Assert.Equal(enemyTarget, enemyOutcome.Target);
        Assert.Equal(enemyTarget, Assert.Single(state.Enemies).Position);
    }

    [Fact]
    public async Task PerformGameActionAsync_PlayerReachesExit_Wins()
    {
        var map = new GameMap(3, 1);
        var exitPosition = new Position(1, 0);
        map.SetSurface(exitPosition, SurfaceType.Exit);
        GameState state = CreateState(
            map,
            new Position(0, 0),
            enemyPosition: new Position(2, 0));
        var manager = new TurnManager(state);

        TurnResult result = await manager.PerformGameActionAsync(Direction.Right);

        Assert.Equal(GameStatus.Won, result.Status);
        Assert.Equal(GameStatus.Won, manager.Status);
        Assert.Equal(exitPosition, result.World.Player.Position);
        Assert.Equal(
            SurfaceType.Exit,
            result.World.Map.GetSurface(result.World.Player.Position).Type);
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => manager.PerformGameActionAsync(Direction.Right));
        Assert.Equal(1, manager.TurnNumber);
    }

    [Fact]
    public async Task PerformGameActionAsync_PlayerKillsLastEnemy_Wins()
    {
        GameState state = CreateState(
            new GameMap(2, 1),
            new Position(0, 0),
            enemyStats: new CreatureStats(1, 1, MovementType.Walking),
            enemyPosition: new Position(1, 0));
        var manager = new TurnManager(state);

        TurnResult result = await manager.PerformGameActionAsync(Direction.Right);

        Assert.Equal(GameStatus.Won, result.Status);
        Assert.Equal(GameStatus.Won, manager.Status);
        Assert.Empty(result.World.Enemies);
        Assert.True(result.World.Player.IsAlive);
    }

    [Fact]
    public async Task PerformGameActionAsync_KilledEnemyFreesCellForMoverWhileAnotherEnemyAttacksPlayer_NoErrors()
    {
        var enemyType = new EnemyType(
            EnemyTypeId.Bat,
            new CreatureStats(3, 2, MovementType.Walking),
            new ChaserBrain());
        var state = new GameState(
            new GameMap(4, 2),
            new EnemyCatalog([enemyType]),
            new Position(1, 1),
            new CreatureStats(10, 3, MovementType.Walking));
        var killedEnemyPosition = new Position(2, 1);
        var movingEnemyStart = new Position(3, 1);
        var attackingEnemyPosition = new Position(1, 0);
        state.AddEnemy(killedEnemyPosition, enemyType.TypeId);
        state.AddEnemy(movingEnemyStart, enemyType.TypeId);
        state.AddEnemy(attackingEnemyPosition, enemyType.TypeId);
        Enemy killedEnemy = Assert.Single(
            state.Enemies,
            enemy => enemy.Position == killedEnemyPosition);
        Enemy movingEnemy = Assert.Single(
            state.Enemies,
            enemy => enemy.Position == movingEnemyStart);
        Enemy attackingEnemy = Assert.Single(
            state.Enemies,
            enemy => enemy.Position == attackingEnemyPosition);
        var manager = new TurnManager(state);

        TurnResult result = await manager.PerformGameActionAsync(Direction.Right);

        Assert.Equal(CreatureActionResult.Attacked, result.PlayerOutcome.Result);
        Assert.Equal(new Position(1, 1), result.PlayerOutcome.From);
        Assert.Equal(killedEnemyPosition, result.PlayerOutcome.Target);
        CreatureDeath killedEnemyEvent = Assert.Single(result.Deaths);
        Assert.Equal(killedEnemy.Id, killedEnemyEvent.CreatureId);
        Assert.Equal(killedEnemyPosition, killedEnemyEvent.Position);

        Assert.Equal(2, result.EnemyOutcomes.Count);
        Assert.Equal(movingEnemy.Id, result.EnemyOutcomes[0].ActorId);
        Assert.Equal(CreatureActionResult.Moved, result.EnemyOutcomes[0].Result);
        Assert.Equal(movingEnemyStart, result.EnemyOutcomes[0].From);
        Assert.Equal(killedEnemyPosition, result.EnemyOutcomes[0].Target);
        Assert.Equal(killedEnemyPosition, movingEnemy.Position);
        Assert.Same(movingEnemy, state.GetCreatureAt(killedEnemyPosition));

        Assert.Equal(attackingEnemy.Id, result.EnemyOutcomes[1].ActorId);
        Assert.Equal(CreatureActionResult.Attacked, result.EnemyOutcomes[1].Result);
        Assert.Equal(attackingEnemyPosition, result.EnemyOutcomes[1].From);
        Assert.Equal(state.Player.Position, result.EnemyOutcomes[1].Target);
        Assert.Equal(8, state.Player.Health);
        Assert.Equal(attackingEnemyPosition, attackingEnemy.Position);
        Assert.DoesNotContain(killedEnemy, state.Enemies);
    }

    [Fact]
    public async Task PerformGameActionAsync_EnemyPhaseCanKillPlayer()
    {
        var map = new GameMap(2, 2);
        var exitPosition = new Position(1, 0);
        map.SetSurface(exitPosition, SurfaceType.Exit);
        GameState state = CreateState(
            map,
            new Position(0, 0),
            new CreatureStats(3, 10, MovementType.Walking),
            new ChaserBrain(),
            new Position(1, 1),
            playerHealth: 1);
        var manager = new TurnManager(state);

        TurnResult result = await manager.PerformGameActionAsync(Direction.Right);

        CreatureActionOutcome enemyOutcome = Assert.Single(result.EnemyOutcomes);
        Assert.Equal(CreatureActionResult.Moved, result.PlayerOutcome.Result);
        Assert.Equal(exitPosition, result.World.Player.Position);
        Assert.False(state.Player.IsAlive);
        Assert.Equal(CreatureActionResult.Attacked, enemyOutcome.Result);
        CreatureDeath playerDeath = Assert.Single(result.Deaths);
        Assert.Equal(state.Player.Id, playerDeath.CreatureId);
        Assert.Equal(state.Player.Position, playerDeath.Position);
        Assert.Equal(1, manager.TurnNumber);
        Assert.Equal(GameStatus.Lost, result.Status);
        Assert.Equal(GameStatus.Lost, manager.Status);
    }

    [Fact]
    public async Task PerformGameActionAsync_MultipleHitsEndingInDeathKeepOnlyDeathEvent()
    {
        var enemyType = new EnemyType(
            EnemyTypeId.Bat,
            new CreatureStats(3, 1, MovementType.Walking),
            new ChaserBrain());
        var state = new GameState(
            new GameMap(3, 2),
            new EnemyCatalog([enemyType]),
            new Position(1, 1),
            new CreatureStats(3, 0, MovementType.Walking));
        state.AddEnemy(new Position(1, 0), enemyType.TypeId);
        state.AddEnemy(new Position(0, 1), enemyType.TypeId);
        state.AddEnemy(new Position(2, 1), enemyType.TypeId);
        var manager = new TurnManager(state);

        TurnResult result = await manager.PerformGameActionAsync(Direction.Down);

        Assert.Equal(3, result.EnemyOutcomes.Count);
        CreatureDeath death = Assert.Single(result.Deaths);
        Assert.Equal(state.Player.Id, death.CreatureId);
        Assert.Equal(state.Player.Position, death.Position);
    }

    [Fact]
    public async Task PerformGameActionAsync_MultipleHitsUpdateFinalHealthWithoutCreatingDeaths()
    {
        var enemyType = new EnemyType(
            EnemyTypeId.Bat,
            new CreatureStats(3, 1, MovementType.Walking),
            new ChaserBrain());
        var state = new GameState(
            new GameMap(3, 2),
            new EnemyCatalog([enemyType]),
            new Position(1, 1),
            new CreatureStats(10, 0, MovementType.Walking));
        state.AddEnemy(new Position(1, 0), enemyType.TypeId);
        state.AddEnemy(new Position(0, 1), enemyType.TypeId);
        state.AddEnemy(new Position(2, 1), enemyType.TypeId);
        var manager = new TurnManager(state);

        TurnResult result = await manager.PerformGameActionAsync(Direction.Down);

        Assert.Equal(3, result.EnemyOutcomes.Count);
        Assert.Empty(result.Deaths);
        Assert.Equal(7, state.Player.Health);
    }

    private static GameState CreateState(
        GameMap map,
        Position playerPosition,
        CreatureStats? enemyStats = null,
        IEnemyBrain? brain = null,
        Position? enemyPosition = null,
        int playerHealth = 10)
    {
        var enemyType = new EnemyType(
            EnemyTypeId.Bat,
            enemyStats ?? new CreatureStats(3, 1, MovementType.Walking),
            brain);
        var state = new GameState(
            map,
            new EnemyCatalog([enemyType]),
            playerPosition,
            new CreatureStats(playerHealth, 1, MovementType.Walking));

        if (enemyPosition is Position position)
            state.AddEnemy(position, enemyType.TypeId);

        return state;
    }
}
