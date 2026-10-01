using RogueMaker.Core.Actions;
using RogueMaker.Core.Creatures;
using RogueMaker.Core.Game;
using RogueMaker.Core.Map;

namespace RogueMaker.Core.Turns;

/// <summary>Applies an already resolved enemy phase to the live game state.</summary>
public static class EnemyTurnExecutor
{
    public static (
        IReadOnlyList<CreatureActionOutcome> Outcomes,
        IReadOnlyList<CreatureDeath> Deaths) Execute(
        GameState state,
        ResolvedEnemyAction[] resolvedActions)
    {
        var outcomes = new CreatureActionOutcome[resolvedActions.Length];
        var moves = new List<(
            CreatureId CreatureId,
            Position From,
            Position To)>(resolvedActions.Length);
        var attacks = new List<int>();
        var deaths = new List<CreatureDeath>();

        for (int index = 0; index < resolvedActions.Length; index++)
        {
            ResolvedEnemyAction resolved = resolvedActions[index];

            switch (resolved.Result)
            {
                case CreatureActionResult.Waited:
                    outcomes[index] = CreateOutcome(
                        resolved,
                        CreatureActionResult.Waited);
                    break;

                case CreatureActionResult.Blocked:
                    outcomes[index] = CreateOutcome(
                        resolved,
                        CreatureActionResult.Blocked);
                    break;

                case CreatureActionResult.Moved:
                    moves.Add((resolved.Plan.EnemyId, resolved.From, resolved.Target));
                    outcomes[index] = CreateOutcome(
                        resolved,
                        CreatureActionResult.Moved);
                    break;

                case CreatureActionResult.Attacked:
                    attacks.Add(index);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(resolvedActions),
                        resolved.Result,
                        "Unknown enemy action resolution.");
            }
        }

        state.ApplyResolvedMoves(moves);

        foreach (int index in attacks)
        {
            ResolvedEnemyAction resolved = resolvedActions[index];
            (CreatureActionOutcome outcome, CreatureDeath? death) =
                state.ApplyEnemyAction(
                    resolved.Plan.EnemyId,
                    resolved.Plan.Action);

            outcomes[index] = outcome;

            if (death is CreatureDeath creatureDeath)
                deaths.Add(creatureDeath);
        }

        return (Array.AsReadOnly(outcomes), deaths.AsReadOnly());
    }

    private static CreatureActionOutcome CreateOutcome(
        ResolvedEnemyAction resolved,
        CreatureActionResult result)
        => new(
            resolved.Plan.EnemyId,
            result,
            resolved.From,
            resolved.Target);
}
