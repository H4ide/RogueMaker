using RogueMaker.Core.Actions;
using RogueMaker.Core.Creatures;
using RogueMaker.Core.Game;
using RogueMaker.Core.Map;
using RogueMaker.Core.Movement;

namespace RogueMaker.Core.Turns;

/// <summary>Resolves enemy intentions deterministically without changing the world.</summary>
public static class EnemyTurnResolver
{
    public static async Task<ResolvedEnemyAction[]> ResolveAsync(
        WorldSnapshot world,
        Task<PlannedEnemyAction>[] planTasks)
    {
        var context = new ResolutionContext(world, planTasks);
        ActionCandidate[] candidates =
            await Task.WhenAll(context.CreateCandidateTasks());

        return Resolve(candidates);
    }

    /// <summary>Creates action candidates for one enemy phase.</summary>
    private sealed class ResolutionContext
    {
        private static readonly Direction[] Directions =
            Enum.GetValues<Direction>();

        private readonly WorldSnapshot _world;
        private readonly Task<PlannedEnemyAction>[] _planTasks;
        private readonly Dictionary<CreatureId, int> _enemyIndexes;

        public ResolutionContext(
            WorldSnapshot world,
            Task<PlannedEnemyAction>[] planTasks)
        {
            _world = world;
            _planTasks = planTasks;
            _enemyIndexes = new Dictionary<CreatureId, int>(
                world.Enemies.Count);

            for (int index = 0; index < world.Enemies.Count; index++)
                _enemyIndexes.Add(world.Enemies[index].Id, index);
        }

        public Task<ActionCandidate>[] CreateCandidateTasks()
        {
            var candidateTasks =
                new Task<ActionCandidate>[_world.Enemies.Count];

            for (int index = 0; index < candidateTasks.Length; index++)
            {
                int enemyIndex = index;
                candidateTasks[index] = Task.Run(
                    () => CreateCandidateAsync(enemyIndex));
            }

            return candidateTasks;
        }

        private async Task<ActionCandidate> CreateCandidateAsync(
            int enemyIndex)
        {
            PlannedEnemyAction plan = await _planTasks[enemyIndex];
            EnemySnapshot enemy = _world.Enemies[enemyIndex];
            return plan.Action switch
            {
                EnemyAction.Wait => new ActionCandidate(
                    plan,
                    enemy.Position,
                    enemy.Position,
                    CreatureActionResult.Waited,
                    null),
                EnemyAction.Move move => await CreateMoveCandidateAsync(
                    enemyIndex,
                    enemy,
                    plan,
                    move.Direction),
                _ => throw new ArgumentOutOfRangeException(
                    nameof(plan),
                    plan.Action,
                    "Unknown enemy action."),
            };
        }

        private async Task<ActionCandidate> CreateMoveCandidateAsync(
            int enemyIndex,
            EnemySnapshot enemy,
            PlannedEnemyAction plan,
            Direction direction)
        {
            Position target = enemy.Position + direction;
            ICreatureView? occupant = _world.GetCreatureAt(target);

            // Moving into the player's cell is an attack, not a movement.
            if (occupant is PlayerSnapshot)
            {
                return new ActionCandidate(
                    plan,
                    enemy.Position,
                    target,
                    CreatureActionResult.Attacked,
                    null);
            }

            if (!MovementQueries.CanEnter(enemy, _world.Map, target))
            {
                return new ActionCandidate(
                    plan,
                    enemy.Position,
                    target,
                    CreatureActionResult.Blocked,
                    null);
            }

            if (!await HasTargetPriorityAsync(enemyIndex, target))
            {
                return new ActionCandidate(
                    plan,
                    enemy.Position,
                    target,
                    CreatureActionResult.Blocked,
                    null);
            }

            return occupant switch
            {
                null => new ActionCandidate(
                    plan,
                    enemy.Position,
                    target,
                    CreatureActionResult.Moved,
                    null),
                EnemySnapshot occupiedEnemy => new ActionCandidate(
                    plan,
                    enemy.Position,
                    target,
                    null,
                    _enemyIndexes[occupiedEnemy.Id]),
                _ => throw new InvalidOperationException(
                    $"Unsupported creature occupies target {target}."),
            };
        }

        private async Task<bool> HasTargetPriorityAsync(
            int enemyIndex,
            Position target)
        {
            foreach (Direction direction in Directions)
            {
                if (_world.GetCreatureAt(target + direction) is not EnemySnapshot contender)
                {
                    continue;
                }

                int contenderIndex = _enemyIndexes[contender.Id];

                if (contenderIndex >= enemyIndex)
                    continue;

                PlannedEnemyAction plan = await _planTasks[contenderIndex];

                if (plan.Action is EnemyAction.Move move
                    && contender.Position + move.Direction == target
                    && MovementQueries.CanEnter(
                        contender,
                        _world.Map,
                        target))
                {
                    return false;
                }
            }

            return true;
        }
    }

    private static ResolvedEnemyAction[] Resolve(
        ActionCandidate[] candidates)
    {
        MoveState[] moveStates = ResolveMoveComponents(candidates);

        var resolvedActions = new ResolvedEnemyAction[candidates.Length];

        for (int index = 0; index < candidates.Length; index++)
        {
            ActionCandidate candidate = candidates[index];
            CreatureActionResult result;

            if (candidate.Result is CreatureActionResult immediateResult)
            {
                result = immediateResult;
            }
            else
            {
                result = moveStates[index] switch
                {
                    MoveState.Succeeded => CreatureActionResult.Moved,
                    MoveState.Blocked => CreatureActionResult.Blocked,
                    _ => throw new InvalidOperationException(
                        "Move candidate was not resolved."),
                };
            }

            resolvedActions[index] = new ResolvedEnemyAction(
                candidate.Plan,
                result,
                candidate.From,
                candidate.Target);
        }

        return resolvedActions;
    }

    private static MoveState[] ResolveMoveComponents(
        ActionCandidate[] candidates)
    {
        var states = new MoveState[candidates.Length];

        for (int index = 0; index < candidates.Length; index++)
        {
            // The candidate has no immediate result and has not been visited,
            // so resolve the movement component that contains it.
            if (candidates[index].Result is null
                && states[index] == MoveState.Unknown)
            {
                ResolveMoveComponent(index, candidates, states);
            }
        }

        return states;
    }

    private static void ResolveMoveComponent(
        int firstEnemyIndex,
        ActionCandidate[] candidates,
        MoveState[] states)
    {
        int enemyIndex = firstEnemyIndex;
        MoveState result;

        // Walk forward until the component reaches a completed move,
        // a non-moving action, an already resolved component, a cycle.
        while (true)
        {
            ActionCandidate candidate = candidates[enemyIndex];

            // Moved means that the occupant will vacate its cell. Any other
            // completed result means that the current chain is blocked.
            if (candidate.Result is not null)
            {
                result = candidate.Result == CreatureActionResult.Moved
                    ? MoveState.Succeeded
                    : MoveState.Blocked;
                break;
            }

            if (states[enemyIndex] != MoveState.Unknown)
            {
                // Visiting means that this traversal reached its own path:
                // swaps and longer cycles are blocked. A final state can be reused.
                result = states[enemyIndex] == MoveState.Visiting
                    ? MoveState.Blocked
                    : states[enemyIndex];
                break;
            }

            states[enemyIndex] = MoveState.Visiting;
            enemyIndex = candidate.OccupantIndex!.Value;
        }

        enemyIndex = firstEnemyIndex;

        // Walk the visited part again and publish the same final result to
        // every movement in the component.
        while (states[enemyIndex] == MoveState.Visiting)
        {
            ActionCandidate candidate = candidates[enemyIndex];
            states[enemyIndex] = result;
            enemyIndex = candidate.OccupantIndex!.Value;
        }
    }

    private readonly record struct ActionCandidate(
        PlannedEnemyAction Plan,
        Position From,
        Position Target,
        CreatureActionResult? Result,
        int? OccupantIndex);

    private enum MoveState
    {
        Unknown,
        Visiting,
        Succeeded,
        Blocked,
    }
}
