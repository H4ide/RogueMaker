using RogueMaker.Core.Movement;

namespace RogueMaker.Core.Map;

/// <summary>
/// Provides read-only map queries.
/// Implementations may represent either a live map or an immutable snapshot of the map.
/// </summary>
public interface IMapView
{
    /// <summary>The number of columns in the map.</summary>
    int Width { get; }

    /// <summary>The number of rows in the map.</summary>
    int Height { get; }

    /// <summary>Returns whether a position lies inside the map bounds.</summary>
    bool Contains(Position position);

    /// <summary>Gets the tile stored at an in-bounds position.</summary>
    Tile GetSurface(Position position);

    /// <summary>Returns whether a movement type may enter a position.</summary>
    bool AllowsEntry(Position position, MovementType movementType);
}
