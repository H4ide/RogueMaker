using RogueMaker.Core.Creatures;
using RogueMaker.Core.Map;

namespace RogueMaker.Core.Game;

/// <summary>An immutable capture of a map version and creature values.</summary>
public sealed class WorldSnapshot
{
    private readonly IReadOnlyDictionary<CreatureId, ICreatureView> _creaturesById;
    private readonly IReadOnlyDictionary<Position, ICreatureView> _creaturesByPosition;

    private WorldSnapshot(
        MapSnapshot map,
        PlayerSnapshot player,
        EnemySnapshot[] enemies)
    {

        Map = map;
        Player = player;
        Enemies = Array.AsReadOnly(enemies);

        ICreatureView[] creatures = [player, .. enemies];
        _creaturesById = creatures.ToDictionary(static creature => creature.Id);
        _creaturesByPosition = creatures.ToDictionary(static creature => creature.Position);
    }

    /// <summary>The immutable map version captured with this world.</summary>
    public MapSnapshot Map { get; }

    /// <summary>The player values captured with this world.</summary>
    public PlayerSnapshot Player { get; }

    /// <summary>All enemy values captured with this world, ordered by ID.</summary>
    public IReadOnlyList<EnemySnapshot> Enemies { get; }

    /// <summary>Gets a captured creature by ID, or <see langword="null"/> when absent.</summary>
    // currently unused, whole _creaturesById is possibly not needed.
    public ICreatureView? GetCreature(CreatureId creatureId)
        => _creaturesById.TryGetValue(creatureId, out ICreatureView? creature)
            ? creature
            : null;

    /// <summary>Gets the captured creature at a position, or <see langword="null"/> when empty.</summary>
    public ICreatureView? GetCreatureAt(Position position)
        => _creaturesByPosition.TryGetValue(position, out ICreatureView? creature)
            ? creature
            : null;

    internal static WorldSnapshot Capture(
        MapSnapshot map,
        IEnumerable<Creature> creatures)
    {
        PlayerSnapshot player = creatures
            .OfType<Player>()
            .Select(PlayerSnapshot.Capture)
            .Single();
        EnemySnapshot[] enemies = creatures
            .OfType<Enemy>()
            .Select(EnemySnapshot.Capture)
            .OrderBy(static enemy => enemy.Id.Value)
            .ToArray();

        return new WorldSnapshot(map, player, enemies);
    }
}
