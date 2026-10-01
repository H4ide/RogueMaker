using RogueMaker.Core.Creatures;
using RogueMaker.Core.Map;

namespace RogueMaker.Core.Game;

/// <summary>Tracks which creature occupies each non-empty map position.</summary>
internal class CreatureOccupancy
{
    private readonly Dictionary<Position, CreatureId> _occupants = [];

    /// <summary>Returns whether a creature already occupies the position.</summary>
    public bool IsOccupied(Position position)
        => _occupants.ContainsKey(position);

    /// <summary>Returns the occupant ID, or <see langword="null"/> for an empty position.</summary>
    public CreatureId? GetOccupant(Position position)
        => _occupants.TryGetValue(position, out CreatureId id) ? id : null;

    /// <summary>Marks a position as occupied by the creature.</summary>
    public void Place(Position position, CreatureId creatureId)
    {
        if (!_occupants.TryAdd(position, creatureId))
            throw new InvalidOperationException($"Position {position} is already occupied.");
    }

    /// <summary>Moves the occupant from one position to an available target position.</summary>
    public void Move(Position from, Position to, CreatureId creatureId)
    {
        if (!_occupants.TryGetValue(from, out CreatureId currentId))
            throw new InvalidOperationException($"Position {from} is not occupied.");

        if (currentId != creatureId)
            throw new InvalidOperationException($"Position {from} is occupied by another creature.");

        if (_occupants.ContainsKey(to))
            throw new InvalidOperationException($"Position {to} is already occupied.");

        _occupants.Remove(from);
        _occupants.Add(to, creatureId);
    }

    /// <summary>Moves several occupants simultaneously after validating the whole batch.</summary>
    public void MoveMany(
        IReadOnlyCollection<(CreatureId CreatureId, Position From, Position To)> moves)
    {
        var movingIds = new HashSet<CreatureId>(moves.Count);
        var targets = new HashSet<Position>(moves.Count);

        foreach ((CreatureId creatureId, Position from, Position to) in moves)
        {
            movingIds.Add(creatureId);

            if (!targets.Add(to))
                throw new InvalidOperationException($"More than one creature is moving to {to}.");

            if (!_occupants.TryGetValue(from, out CreatureId currentId))
                throw new InvalidOperationException($"Position {from} is not occupied.");

            if (currentId != creatureId)
                throw new InvalidOperationException($"Position {from} is occupied by another creature.");
        }

        foreach ((_, _, Position to) in moves)
        {
            if (_occupants.TryGetValue(to, out CreatureId occupantId)
                && !movingIds.Contains(occupantId))
            {
                throw new InvalidOperationException($"Position {to} is occupied by a creature that is not moving.");
            }
        }

        foreach ((_, Position from, _) in moves)
            _occupants.Remove(from);

        foreach ((CreatureId creatureId, _, Position to) in moves)
            _occupants.Add(to, creatureId);
    }

    /// <summary>Removes the occupant from a position.</summary>
    public void Remove(Position position, CreatureId creatureId)
    {
        if (!_occupants.TryGetValue(position, out CreatureId currentId))
            throw new InvalidOperationException($"Position {position} is not occupied.");

        if (currentId != creatureId)
            throw new InvalidOperationException($"Position {position} is occupied by another creature.");

        _occupants.Remove(position);
    }
}
