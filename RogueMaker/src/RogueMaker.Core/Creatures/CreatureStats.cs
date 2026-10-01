using RogueMaker.Core.Movement;

namespace RogueMaker.Core.Creatures;

/// <summary>
/// Contains the shared combat and movement stats of a creature type.
/// </summary>
public sealed class CreatureStats
{
    /// <summary>
    /// Creates a set of creature stats.
    /// </summary>
    /// <param name="maxHealth">The maximum health; must be positive.</param>
    /// <param name="baseDamage">The base attack damage. cannot be negative.</param>
    /// <param name="movementType">How the creature moves.</param>
    public CreatureStats(int maxHealth, int baseDamage, MovementType movementType)
    {
        if (maxHealth <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxHealth), maxHealth, "Max health must be positive.");

        if (baseDamage < 0)
            throw new ArgumentOutOfRangeException(nameof(baseDamage), baseDamage, "Base damage cannot be negative.");

        MaxHealth = maxHealth;
        BaseDamage = baseDamage;
        MovementType = movementType;
    }

    /// <summary>The maximum and starting health.</summary>
    public int MaxHealth { get; }

    /// <summary>The damage dealt by a basic attack before equipment or effects.</summary>
    public int BaseDamage { get; }

    /// <summary>How the creature moves across surfaces.</summary>
    public MovementType MovementType { get; }
}
