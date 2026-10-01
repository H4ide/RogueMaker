using RogueMaker.Core.Actions;
using RogueMaker.Core.Creatures;
using RogueMaker.Core.Enemies;
using RogueMaker.Core.Game;
using RogueMaker.Core.Map;
using RogueMaker.Core.Movement;
using RogueMaker.Core.Turns;

namespace RogueMaker.Core.Tests;

public class EnemyTurnResolverTests
{
    [Fact]
    public async Task Resolve_TwoEnemiesClaimSameCell_LowerIdWins()
    {
        GameState state = CreateState(
            new GameMap(3, 2),
            new Position(1, 0),
            new Position(0, 1),
            new Position(2, 1));
        List<Enemy> enemies = GetEnemiesById(state);
        var plans = new PlannedEnemyAction[]
        {
            new(enemies[0].Id, new EnemyAction.Move(Direction.Right)),
            new(enemies[1].Id, new EnemyAction.Move(Direction.Left)),
        };

        IReadOnlyList<CreatureActionOutcome> outcomes =
            (await ResolveAndExecuteAsync(state, plans)).Outcomes;

        Assert.Equal(enemies[0].Id, outcomes[0].ActorId);
        Assert.Equal(enemies[1].Id, outcomes[1].ActorId);
        Assert.Equal(CreatureActionResult.Moved, outcomes[0].Result);
        Assert.Equal(CreatureActionResult.Blocked, outcomes[1].Result);
        Assert.Equal(new Position(1, 1), enemies[0].Position);
        Assert.Equal(new Position(2, 1), enemies[1].Position);
    }

    [Fact]
    public async Task Resolve_ChainEndingInEmptyCell_MovesWholeChain()
    {
        GameState state = CreateState(
            new GameMap(5, 1),
            new Position(0, 0),
            new Position(1, 0),
            new Position(2, 0),
            new Position(3, 0));
        List<Enemy> enemies = GetEnemiesById(state);
        var plans = new PlannedEnemyAction[enemies.Count];

        for (int i = 0; i < enemies.Count; i++)
        {
            plans[i] = new PlannedEnemyAction(
                enemies[i].Id,
                new EnemyAction.Move(Direction.Right));
        }

        IReadOnlyList<CreatureActionOutcome> outcomes =
            (await ResolveAndExecuteAsync(state, plans)).Outcomes;

        Assert.All(outcomes, outcome => Assert.Equal(CreatureActionResult.Moved, outcome.Result));
        Assert.Equal(new Position(2, 0), enemies[0].Position);
        Assert.Equal(new Position(3, 0), enemies[1].Position);
        Assert.Equal(new Position(4, 0), enemies[2].Position);
        Assert.Null(state.GetCreatureAt(new Position(1, 0)));
        Assert.Same(enemies[0], state.GetCreatureAt(new Position(2, 0)));
        Assert.Same(enemies[1], state.GetCreatureAt(new Position(3, 0)));
        Assert.Same(enemies[2], state.GetCreatureAt(new Position(4, 0)));
    }

    [Fact]
    public async Task Resolve_TargetOccupantWaits_MoverIsBlocked()
    {
        GameState state = CreateState(
            new GameMap(3, 1),
            new Position(0, 0),
            new Position(1, 0),
            new Position(2, 0));
        List<Enemy> enemies = GetEnemiesById(state);
        var plans = new PlannedEnemyAction[]
        {
            new(enemies[0].Id, new EnemyAction.Move(Direction.Right)),
            new(enemies[1].Id, new EnemyAction.Wait()),
        };

        IReadOnlyList<CreatureActionOutcome> outcomes =
            (await ResolveAndExecuteAsync(state, plans)).Outcomes;

        Assert.Equal(enemies[0].Id, outcomes[0].ActorId);
        Assert.Equal(CreatureActionResult.Blocked, outcomes[0].Result);
        Assert.Equal(enemies[1].Id, outcomes[1].ActorId);
        Assert.Equal(CreatureActionResult.Waited, outcomes[1].Result);
        Assert.Equal(new Position(1, 0), enemies[0].Position);
        Assert.Equal(new Position(2, 0), enemies[1].Position);
    }

    [Fact]
    public async Task Resolve_TwoEnemiesSwapCells_BlocksBoth()
    {
        GameState state = CreateState(
            new GameMap(3, 1),
            new Position(0, 0),
            new Position(1, 0),
            new Position(2, 0));
        List<Enemy> enemies = GetEnemiesById(state);
        var plans = new PlannedEnemyAction[]
        {
            new(enemies[0].Id, new EnemyAction.Move(Direction.Right)),
            new(enemies[1].Id, new EnemyAction.Move(Direction.Left)),
        };

        IReadOnlyList<CreatureActionOutcome> outcomes =
            (await ResolveAndExecuteAsync(state, plans)).Outcomes;

        Assert.All(outcomes, outcome => Assert.Equal(CreatureActionResult.Blocked, outcome.Result));
        Assert.Equal(new Position(1, 0), enemies[0].Position);
        Assert.Equal(new Position(2, 0), enemies[1].Position);
    }

    [Fact]
    public async Task Resolve_MoveCycle_BlocksEveryEnemyInCycle()
    {
        GameState state = CreateState(
            new GameMap(3, 3),
            new Position(0, 0),
            new Position(1, 1),
            new Position(1, 2),
            new Position(2, 2),
            new Position(2, 1));
        List<Enemy> enemies = GetEnemiesById(state);
        var plans = new PlannedEnemyAction[]
        {
            new(enemies[0].Id, new EnemyAction.Move(Direction.Down)),
            new(enemies[1].Id, new EnemyAction.Move(Direction.Right)),
            new(enemies[2].Id, new EnemyAction.Move(Direction.Up)),
            new(enemies[3].Id, new EnemyAction.Move(Direction.Left)),
        };

        IReadOnlyList<CreatureActionOutcome> outcomes =
            (await ResolveAndExecuteAsync(state, plans)).Outcomes;

        Assert.All(outcomes, outcome => Assert.Equal(CreatureActionResult.Blocked, outcome.Result));
        Assert.Equal(new Position(1, 1), enemies[0].Position);
        Assert.Equal(new Position(1, 2), enemies[1].Position);
        Assert.Equal(new Position(2, 2), enemies[2].Position);
        Assert.Equal(new Position(2, 1), enemies[3].Position);
    }

    private static async Task<(
        IReadOnlyList<CreatureActionOutcome> Outcomes,
        IReadOnlyList<CreatureDeath> Deaths)> ResolveAndExecuteAsync(
        GameState state,
        PlannedEnemyAction[] plans)
    {
        ResolvedEnemyAction[] resolvedActions =
            await ResolveAsync(state, plans);

        return EnemyTurnExecutor.Execute(state, resolvedActions);
    }

    private static async Task<ResolvedEnemyAction[]> ResolveAsync(
        GameState state,
        PlannedEnemyAction[] plans)
    {
        var planTasks = new Task<PlannedEnemyAction>[plans.Length];

        for (int index = 0; index < plans.Length; index++)
            planTasks[index] = Task.FromResult(plans[index]);

        return await EnemyTurnResolver.ResolveAsync(
            state.CreateSnapshot(),
            planTasks);
    }

    private static GameState CreateState(
        GameMap map,
        Position playerPosition,
        params Position[] enemyPositions)
    {
        var enemyType = new EnemyType(
            EnemyTypeId.Bat,
            new CreatureStats(3, 1, MovementType.Walking));
        var state = new GameState(
            map,
            new EnemyCatalog([enemyType]),
            playerPosition,
            new CreatureStats(10, 2, MovementType.Walking));

        foreach (Position position in enemyPositions)
            state.AddEnemy(position, enemyType.TypeId);

        return state;
    }

    private static List<Enemy> GetEnemiesById(GameState state)
    {
        var enemies = new List<Enemy>(state.Enemies.Count);

        foreach (Enemy enemy in state.Enemies)
            enemies.Add(enemy);

        enemies.Sort(static (left, right) => left.Id.Value.CompareTo(right.Id.Value));
        return enemies;
    }
}
