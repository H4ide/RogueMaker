using RogueMaker.Core.Map;

namespace RogueMaker.Core.Creatures;

/// <summary>
/// Provides read-only access to creature values.
/// Implementations may represent either a live creature or an immutable snapshot.
/// </summary>
public interface ICreatureView
{
    /// <summary>The stable creature identifier.</summary>
    CreatureId Id { get; }

    /// <summary>The side controlling the creature.</summary>
    CreatureSide Side { get; }

    /// <summary>The creature position exposed by this view.</summary>
    Position Position { get; }

    /// <summary>The creature health exposed by this view.</summary>
    int Health { get; }

    /// <summary>Whether the creature has health remaining.</summary>
    bool IsAlive { get; }

    /// <summary>The creature's immutable combat and movement characteristics.</summary>
    CreatureStats Stats { get; }
}
