using RogueMaker.Core.Actions;
using RogueMaker.Core.Creatures;
using RogueMaker.Core.Game;
using RogueMaker.Core.Map;
using RogueMaker.Core.Movement;

namespace RogueMaker.Core.AI;

/// <summary>Moves toward the player if nearby, prefer horizontal if not on the same column.
/// spends a turn preparing between attempts to move/wait.
/// </summary>
public sealed record ChaserBrain(ChaserState State) : IEnemyBrain
{
    private const int ChaseDistance = 10;

    public ChaserBrain()
        : this(ChaserState.Ready)
    {
    }

    public EnemyAction Decide(WorldSnapshot world, EnemySnapshot enemy)
    {
        ArgumentNullException.ThrowIfNull(world);
        ArgumentNullException.ThrowIfNull(enemy);

        if (!enemy.IsAlive || !world.Player.IsAlive)
            return new EnemyAction.Wait();

        return State switch
        {
            ChaserState.Preparing => new EnemyAction.Wait(),
            ChaserState.Ready => DecideReady(world, enemy),
            _ => throw new InvalidOperationException($"Unknown chaser state: {State}."),
        };
    }

    public IEnemyBrain GetNextBrain(
        EnemyAction plannedAction,
        CreatureActionOutcome outcome)
    {
        ArgumentNullException.ThrowIfNull(plannedAction);

        bool chaseAttemptSucceeded = plannedAction is EnemyAction.Move
            && outcome.Result is (
                CreatureActionResult.Moved
                or CreatureActionResult.Attacked
                or CreatureActionResult.Blocked);

        return State switch
        {
            ChaserState.Preparing => new ChaserBrain(ChaserState.Ready),
            ChaserState.Ready when chaseAttemptSucceeded
                => new ChaserBrain(ChaserState.Preparing),
            ChaserState.Ready => this,
            _ => throw new InvalidOperationException($"Unknown chaser state: {State}."),
        };
    }

    private static EnemyAction DecideReady(
        WorldSnapshot world,
        EnemySnapshot enemy)
    {
        PlayerSnapshot player = world.Player;

        if (enemy.Position.ManhattanDistanceTo(player.Position) > ChaseDistance)
            return new EnemyAction.Wait();

        Direction? firstDirectionTowardPlayer = null;
        foreach (Direction direction in DirectionsToward(enemy.Position, player.Position))
        {
            firstDirectionTowardPlayer ??= direction;

            if (MovementQueries.CanEnter(enemy, world.Map, enemy.Position + direction))
                return new EnemyAction.Move(direction);
        }

        // Preserve the attempted direction when every route toward the player is
        // blocked by a surface. The resolver will produce a Blocked outcome whose
        // target lets the view animate the enemy bumping into that surface.
        return firstDirectionTowardPlayer is Direction blockedDirection
            ? new EnemyAction.Move(blockedDirection)
            : new EnemyAction.Wait();
    }

    private static IEnumerable<Direction> DirectionsToward(Position from, Position to)
    {
        int deltaX = to.X - from.X;
        int deltaY = to.Y - from.Y;
        Direction? horizontal = DirectionFromDelta(deltaX, Direction.Left, Direction.Right);
        Direction? vertical = DirectionFromDelta(deltaY, Direction.Up, Direction.Down);

        if (horizontal is Direction horizontalDirection)
            yield return horizontalDirection;

        if (vertical is Direction verticalDirection)
            yield return verticalDirection;
    }

    private static Direction? DirectionFromDelta(
        int delta,
        Direction negative,
        Direction positive)
        => delta switch
        {
            < 0 => negative,
            > 0 => positive,
            _ => null,
        };
}
