using RogueMaker.Core.AI;
using RogueMaker.Core.Creatures;

namespace RogueMaker.Core.Enemies;

/// <summary>
/// class for an enemy type registered in the catalog.
/// Its identifier and shared stats are fixed when the type is created.
/// </summary>
/// <remarks>
/// EnemyType is a class instead of an interface because it owns the immutable ID and stats
/// shared by every enemy of this type, instead of let each implementation provide
/// values that could change between calls.
/// </remarks>
public sealed class EnemyType
{
    /// <summary>Initializes the fixed data shared by all enemies of this type.</summary>
    public EnemyType(
        EnemyTypeId typeId,
        CreatureStats stats,
        IEnemyBrain? brain = null)
    {
        if (!Enum.IsDefined(typeId))
        {
            throw new ArgumentOutOfRangeException(
                nameof(typeId),
                typeId,
                "Enemy type ID must be a defined value.");
        }

        ArgumentNullException.ThrowIfNull(stats);

        TypeId = typeId;
        Stats = stats;
        InitialBrain = brain ?? WaitBrain.Instance;
    }

    /// <summary>The stable identifier used by catalogs and saved levels.</summary>
    public EnemyTypeId TypeId { get; }

    /// <summary>The shared combat and movement characteristics of this enemy type.</summary>
    public CreatureStats Stats { get; }

    /// <summary>The immutable initial brain assigned to enemies of this type.</summary>
    internal IEnemyBrain InitialBrain { get; }
}
