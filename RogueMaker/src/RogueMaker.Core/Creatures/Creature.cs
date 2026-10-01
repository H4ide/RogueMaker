using RogueMaker.Core.Map;

namespace RogueMaker.Core.Creatures;

/// <summary>
/// Base class for a living creature placed in the game world. 
/// Contains its stats, current hp, position on the map and an id of this creature.
/// </summary>
public abstract class Creature : ICreatureView
{
    /// <summary>
    /// Initializes the state shared by players and enemies or any other creatures.
    /// </summary>
    // Restricted to Core subclasses so external code cannot bypass GameState ID assignment.
    private protected Creature(CreatureId id, Position position, CreatureStats stats)
    {
        // possible null: CreatureStats stats  = null;
        ArgumentNullException.ThrowIfNull(stats);

        Id = id;
        Position = position;
        Stats = stats;
        Health = stats.MaxHealth;
    }

    /// <summary>The id of this particular creature instance.</summary>
    public CreatureId Id { get; }

    /// <summary>The creature's current position on the map.</summary>
    public Position Position { get; private set; }

    /// <summary>The stats used by this creature.</summary>
    public CreatureStats Stats { get; }

    /// <summary>The creature's current health.</summary>
    public int Health { get; private set; }

    /// <summary>Whether the creature still has health remaining.</summary>
    public bool IsAlive => Health > 0;

    /// <summary>The side to which this creature currently belongs.</summary>
    public abstract CreatureSide Side { get; }

    /// <summary>Updates the position after the game state has validated a move.</summary>
    internal void SetPosition(Position position)
        => Position = position;

    /// <summary>Reduces current health without allowing it to become negative.</summary>
    internal void TakeDamage(int damage)
    {
        if (damage < 0)
            throw new ArgumentOutOfRangeException(nameof(damage), damage, "Damage cannot be negative.");

        Health = Math.Max(0, Health - damage);
    }
}
