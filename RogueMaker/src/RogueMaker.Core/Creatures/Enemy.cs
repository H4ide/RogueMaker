using RogueMaker.Core.Actions;
using RogueMaker.Core.AI;
using RogueMaker.Core.Enemies;
using RogueMaker.Core.Game;
using RogueMaker.Core.Map;

namespace RogueMaker.Core.Creatures;

/// <summary>
/// A creature created using an enemy type. Controlled by AI. Player's enemy.
/// </summary>
public class Enemy : Creature
{
    /// <summary>Creates an enemy at the specified position.</summary>
    // Internal so enemies are created and assigned an ID only through GameState (so,only inside Core).
    internal Enemy(CreatureId id, Position position, EnemyType enemyType)
        : base(id, position, GetStats(enemyType))
    {
        EnemyType = enemyType;
        Brain = enemyType.InitialBrain;
    }

    /// <summary>The type of enemy from which this enemy was created.</summary>
    public EnemyType EnemyType { get; }

    /// <summary>The stable type id of this enemy.</summary>
    public EnemyTypeId TypeId => EnemyType.TypeId;

    /// <summary>The immutable brain currently owned by this enemy.</summary>
    internal IEnemyBrain Brain { get; private set; }

    /// <inheritdoc/>
    public override CreatureSide Side => CreatureSide.Enemy;

    /// <summary>Advances the brain after this enemy's action has been resolved.</summary>
    internal void ApplyOutcome(
        EnemyAction plannedAction,
        CreatureActionOutcome outcome)
    {

        if (outcome.ActorId != Id)
        {
            throw new ArgumentException(
                $"Outcome for creature {outcome.ActorId} cannot complete enemy {Id}.",
                nameof(outcome));
        }

        Brain = Brain.GetNextBrain(plannedAction, outcome)
            ?? throw new InvalidOperationException(
                $"Enemy brain {Brain.GetType().Name} returned no next brain.");
    }

    private static CreatureStats GetStats(EnemyType enemyType)
    {
        return enemyType.Stats;
    }
}
