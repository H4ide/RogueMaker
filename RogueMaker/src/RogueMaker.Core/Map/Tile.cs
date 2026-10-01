using RogueMaker.Core.Movement;

namespace RogueMaker.Core.Map;

/// <summary>
/// Contains information how a surface of a tile behaves in terms of movement.
/// </summary>
/// <param name="Type">The gameplay category of the surface of the cell.</param>
public readonly record struct Tile(SurfaceType Type)
{
    /// <summary>
    /// Returns whether the specified type of movement can enter this tile.
    /// </summary>
    /// <param name="movementType">The type of movement to check.</param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="movementType"/> can enter this tile;
    /// otherwise <see langword="false"/>.
    /// </returns>
    public bool Allows(MovementType movementType)
        => movementType switch
        {
            MovementType.Walking => Type is SurfaceType.Floor or SurfaceType.Exit,
            MovementType.Flying => Type is SurfaceType.Floor or SurfaceType.Exit or SurfaceType.Pit,
            _ => throw new ArgumentOutOfRangeException(
                nameof(movementType),
                movementType,
                "Unknown movement type."),
        };
}
