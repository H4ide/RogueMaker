using RogueMaker.Core.Creatures;
using RogueMaker.Core.Enemies;
using RogueMaker.Core.Game;
using RogueMaker.Core.Levels;
using RogueMaker.Core.Map;

namespace RogueMaker.Core.Turns;

/// <summary>Runs the player phase and the following enemy phase.</summary>
public sealed class TurnManager
{
    private readonly GameState _state;

    public TurnManager(GameState state)
    {
        ArgumentNullException.ThrowIfNull(state);
        _state = state;
    }

    /// <summary>
    /// Creates a new game from a fully configured level draft.
    /// The mutable game state remains encapsulated by the returned turn manager.
    /// </summary>
    public static TurnManager Create(LevelDraft level)
    {
        SurfaceType[] surfaces = new SurfaceType[checked(level.Width * level.Height)];

        for (int y = 0; y < level.Height; y++)
        {
            for (int x = 0; x < level.Width; x++)
            {
                int index = (y * level.Width) + x;
                surfaces[index] = level.GetSurface(new Position(x, y));
            }
        }

        var map = new GameMap(
            level.Width,
            level.Height,
            surfaces);
        var state = new GameState(
            map,
            GameContent.EnemyCatalog,
            level.PlayerPosition,
            GameContent.PlayerStats);

        foreach ((Position position, EnemyTypeId enemyType) in level.Enemies)
            state.AddEnemy(position, enemyType);

        return new TurnManager(state);
    }

    /// <summary>Number of player actions that have consumed a turn.</summary>
    public int TurnNumber { get; private set; }

    /// <summary>Current result of the running game.</summary>
    public GameStatus Status { get; private set; } = GameStatus.InProgress;

    /// <summary>
    /// Captures the current world without exposing the mutable game state.
    /// </summary>
    public WorldSnapshot CreateWorldSnapshot()
        => _state.CreateSnapshot();

    /// <summary>Applies one player movement intent and then runs the enemy phase.</summary>
    public async Task<TurnResult> PerformGameActionAsync(Direction direction)
    {
        if (Status != GameStatus.InProgress)
            throw new InvalidOperationException("The Game is Over.");

        (CreatureActionOutcome playerOutcome, CreatureDeath? deathByPlayer) =
            _state.ApplyPlayerAction(direction);
        TurnNumber++;

        WorldSnapshot worldAfterPlayer = _state.CreateSnapshot();
        Task<PlannedEnemyAction>[] planTasks =
            EnemyTurnPlanner.Plan(worldAfterPlayer);
        ResolvedEnemyAction[] resolvedActions =
            await EnemyTurnResolver.ResolveAsync(
                worldAfterPlayer,
                planTasks);
        (IReadOnlyList<CreatureActionOutcome> enemyOutcomes,
            IReadOnlyList<CreatureDeath> deathsByEnemies) =
            EnemyTurnExecutor.Execute(_state, resolvedActions);

        AdjustEnemyBrains(resolvedActions, enemyOutcomes);

        var deaths = new List<CreatureDeath>(deathsByEnemies.Count + 1);

        if (deathByPlayer is CreatureDeath death)
            deaths.Add(death);

        deaths.AddRange(deathsByEnemies);
        WorldSnapshot worldAfterTurn = _state.CreateSnapshot();
        Status = DetermineStatus(worldAfterTurn);

        return new TurnResult(
            TurnNumber,
            playerOutcome,
            enemyOutcomes,
            deaths.AsReadOnly(),
            worldAfterTurn,
            Status);
    }

    private static GameStatus DetermineStatus(WorldSnapshot world)
    {
        if (!world.Player.IsAlive)
            return GameStatus.Lost;

        if (world.Enemies.Count == 0
            || world.Map.GetSurface(world.Player.Position).Type == SurfaceType.Exit)
        {
            return GameStatus.Won;
        }

        return GameStatus.InProgress;
    }

    /// <summary>Adjust enemy brains after all planned actions are resolved.</summary>
    private void AdjustEnemyBrains(
        ResolvedEnemyAction[] resolvedActions,
        IReadOnlyList<CreatureActionOutcome> outcomes)
    {
        for (int index = 0; index < resolvedActions.Length; index++)
        {
            PlannedEnemyAction plan = resolvedActions[index].Plan;
            CreatureActionOutcome outcome = outcomes[index];
            Enemy enemy = _state.GetEnemy(plan.EnemyId)!;

            enemy.ApplyOutcome(plan.Action, outcome);
        }
    }
}
