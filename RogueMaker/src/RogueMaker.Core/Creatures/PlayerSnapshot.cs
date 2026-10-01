using RogueMaker.Core.Map;

namespace RogueMaker.Core.Creatures;

/// <summary>Immutable player values captured at a point in time.</summary>
public sealed class PlayerSnapshot : ICreatureView
{
    private PlayerSnapshot(
        CreatureId id,
        Position position,
        int health,
        CreatureStats stats)
    {
        Id = id;
        Position = position;
        Health = health;
        Stats = stats;
    }

    public CreatureId Id { get; }

    public CreatureSide Side => CreatureSide.Player;

    public Position Position { get; }

    public int Health { get; }

    public bool IsAlive => Health > 0;

    public CreatureStats Stats { get; }

    internal static PlayerSnapshot Capture(Player player)
    {
        ArgumentNullException.ThrowIfNull(player);

        return new PlayerSnapshot(
            player.Id,
            player.Position,
            player.Health,
            player.Stats);
    }
}
