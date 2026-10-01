using RogueMaker.Core.Actions;
using RogueMaker.Core.Creatures;
using RogueMaker.Core.Game;

namespace RogueMaker.Core.AI;

/// <summary>Chooses an enemy action from an immutable world snapshot.</summary>
/// /// <remarks>
/// Implementations must be immutable and thread-safe.
/// Decide must not mutate the brain or captured world.
/// GetNextBrain must return an immutable brain for the next turn.
/// </remarks>
public interface IEnemyBrain
{
    /// <summary>Chooses an action without changing the brain or the world.</summary>
    EnemyAction Decide(WorldSnapshot world, EnemySnapshot enemy);

    /// <summary>Returns the brain for the next turn after an action is resolved.</summary>
    IEnemyBrain GetNextBrain(
        EnemyAction plannedAction,
        CreatureActionOutcome outcome);
}
