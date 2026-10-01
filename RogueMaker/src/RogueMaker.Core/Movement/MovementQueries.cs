using RogueMaker.Core.Creatures;
using RogueMaker.Core.Map;

namespace RogueMaker.Core.Movement;

/// <summary> movement queries for creatures.</summary>
public static class MovementQueries
{
    /// <summary>
    /// Returns whether the creature's movement type may enter the specified map position.
    /// Creature occupancy is intentionally not considered.
    /// </summary>
    public static bool CanEnter(
        ICreatureView creature,
        IMapView map,
        Position position)
    {
        ArgumentNullException.ThrowIfNull(creature);
        ArgumentNullException.ThrowIfNull(map);

        return map.AllowsEntry(position, creature.Stats.MovementType);
    }

    /// <summary>
    /// Returns whether the creature can move to a position in the same row or column
    /// by entering every surface along the straight path.
    /// Creature occupancy is intentionally not considered.
    /// </summary>
    public static bool CanMoveToHorizontalOrVertical(
        ICreatureView creature,
        IMapView map,
        Position position)
    {
        ArgumentNullException.ThrowIfNull(creature);
        ArgumentNullException.ThrowIfNull(map);

        Position from = creature.Position;

        if (!map.Contains(from) || !map.Contains(position))
            return false;

        int deltaX = position.X - from.X;
        int deltaY = position.Y - from.Y;

        if (deltaX != 0 && deltaY != 0)
            return false;

        if (deltaX == 0 && deltaY == 0)
            return true;

        Direction direction = deltaX switch
        {
            < 0 => Direction.Left,
            > 0 => Direction.Right,
            _ => deltaY < 0 ? Direction.Up : Direction.Down,
        };
        Position current = from + direction;

        while (current != position)
        {
            if (!CanEnter(creature, map, current))
                return false;

            current += direction;
        }

        return CanEnter(creature, map, position);
    }
}
