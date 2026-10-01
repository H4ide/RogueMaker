using RogueMaker.Core.Actions;
using RogueMaker.Core.Creatures;
using RogueMaker.Core.Game;

namespace RogueMaker.Core.AI;

/// <summary>An enemy brain that always waits.</summary>
public sealed class WaitBrain : IEnemyBrain
{
    private WaitBrain()
    {
    }

    public static WaitBrain Instance { get; } = new();

    public EnemyAction Decide(WorldSnapshot world, EnemySnapshot enemy)
    {
        ArgumentNullException.ThrowIfNull(world);
        return new EnemyAction.Wait();
    }

    public IEnemyBrain GetNextBrain(
        EnemyAction plannedAction,
        CreatureActionOutcome outcome)
    {
        return this;
    }
}
