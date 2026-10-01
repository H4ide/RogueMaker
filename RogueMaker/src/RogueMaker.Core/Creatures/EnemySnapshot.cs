using RogueMaker.Core.AI;
using RogueMaker.Core.Enemies;
using RogueMaker.Core.Map;

namespace RogueMaker.Core.Creatures;

/// <summary>Immutable enemy values and brain captured at a point in time.</summary>
public sealed class EnemySnapshot : ICreatureView
{
    private EnemySnapshot(
        CreatureId id,
        EnemyTypeId typeId,
        Position position,
        int health,
        CreatureStats stats,
        IEnemyBrain brain)
    {
        Id = id;
        TypeId = typeId;
        Position = position;
        Health = health;
        Stats = stats;
        Brain = brain;
    }

    public CreatureId Id { get; }

    public EnemyTypeId TypeId { get; }

    public CreatureSide Side => CreatureSide.Enemy;

    public Position Position { get; }

    public int Health { get; }

    public bool IsAlive => Health > 0;

    public CreatureStats Stats { get; }

    /// <summary>The immutable brain captured for enemy-turn planning.</summary>
    internal IEnemyBrain Brain { get; }

    internal static EnemySnapshot Capture(Enemy enemy)
    {
        ArgumentNullException.ThrowIfNull(enemy);

        return new EnemySnapshot(
            enemy.Id,
            enemy.TypeId,
            enemy.Position,
            enemy.Health,
            enemy.Stats,
            enemy.Brain);
    }
}
