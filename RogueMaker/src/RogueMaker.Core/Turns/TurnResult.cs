using RogueMaker.Core.Game;

namespace RogueMaker.Core.Turns;

/// <summary>Contains everything that happened during one complete game turn.</summary>
public sealed class TurnResult(
    int turnNumber,
    CreatureActionOutcome playerOutcome,
    IReadOnlyList<CreatureActionOutcome> enemyOutcomes,
    IReadOnlyList<CreatureDeath> deaths,
    WorldSnapshot world,
    GameStatus status)
{
    public int TurnNumber { get; } = turnNumber;

    /// <summary>The game status after the complete turn.</summary>
    public GameStatus Status { get; } = status;

    public CreatureActionOutcome PlayerOutcome { get; } = playerOutcome;
    public IReadOnlyList<CreatureActionOutcome> EnemyOutcomes { get; } = enemyOutcomes;

    /// <summary>The immutable world state after the complete turn.</summary>
    public WorldSnapshot World { get; } = world;

    /// <summary>
    /// Creatures that died during this turn.
    /// </summary>
    public IReadOnlyList<CreatureDeath> Deaths { get; } = deaths;
}
