using RogueMaker.Core.Movement;

namespace RogueMaker.Core.Map;

/// <summary>Class contains an immutable snapshot of the map's surface. If the live map changes, old snapshot is not affected.</summary>
public sealed class MapSnapshot : IMapView
{
    private readonly Tile[] _surfaces;

    internal MapSnapshot(int width, int height, Tile[] surfaces)
    {
        ArgumentNullException.ThrowIfNull(surfaces);

        if (width <= 0)
            throw new ArgumentOutOfRangeException(nameof(width), "Width must be positive.");

        if (height <= 0)
            throw new ArgumentOutOfRangeException(nameof(height), "Height must be positive.");

        if (surfaces.Length != checked(width * height))
        {
            throw new ArgumentException(
                "Surface count must match the map dimensions.",
                nameof(surfaces));
        }

        Width = width;
        Height = height;
        _surfaces = surfaces;
    }

    /// <inheritdoc/>
    public int Width { get; }

    /// <inheritdoc/>
    public int Height { get; }

    /// <inheritdoc/>
    public bool Contains(Position position)
        => position.X >= 0
            && position.X < Width
            && position.Y >= 0
            && position.Y < Height;

    /// <inheritdoc/> 
    public Tile GetSurface(Position position)
        => _surfaces[GetIndex(position)];

    /// <inheritdoc/>
    public bool AllowsEntry(Position position, MovementType movementType)
        => Contains(position) && GetSurface(position).Allows(movementType);

    internal MapSnapshot WithSurface(Position position, SurfaceType type)
    {
        int index = GetIndex(position);
        Tile[] surfaces = (Tile[])_surfaces.Clone();
        surfaces[index] = new Tile(type);
        return new MapSnapshot(Width, Height, surfaces);
    }

    private int GetIndex(Position position)
    {
        if (!Contains(position))
        {
            throw new ArgumentOutOfRangeException(
                nameof(position),
                position,
                "Position must be inside the map.");
        }

        return (position.Y * Width) + position.X;
    }
}
