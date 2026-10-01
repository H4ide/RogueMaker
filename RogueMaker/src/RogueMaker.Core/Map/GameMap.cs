using RogueMaker.Core.Movement;

namespace RogueMaker.Core.Map;

/// <summary>
/// Represents the surface live map of a level.
/// </summary>
/// <remarks>
/// <see cref="GetSurface"/> and <see cref="SetSurface"/> are direct accessors that throw
/// <see cref="ArgumentOutOfRangeException"/> for out-of-bounds positions, but
/// <see cref="AllowsEntry"/> is a safe movement query that returns <see langword="false"/>
/// for positions outside of the map (so a creature at the edge of the map trying to leave simply stays standing).
/// </remarks>
public sealed class GameMap : IMapView
{
    /// <summary>The largest supported number of columns.</summary>
    public const int MaxWidth = 100;

    /// <summary>The largest supported number of rows.</summary>
    public const int MaxHeight = 100;

    /// <summary>
    /// The immutable surface version currently used by this map.
    /// </summary>
    private MapSnapshot _surfaceVersion;

    /// <inheritdoc/>
    public int Width => _surfaceVersion.Width;

    /// <inheritdoc/>
    public int Height => _surfaceVersion.Height;

    /// <summary>
    /// Creates a map of the requested size and fills it with floor tiles.
    /// <paramref name="width"/> must be in the range [1, <see cref="MaxWidth"/>].
    /// <paramref name="height"/> must be in the range [1, <see cref="MaxHeight"/>].
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown if at least one of width or height is outside the valid range.
    /// </exception>
    public GameMap(int width, int height)
    {
        ValidateDimensions(width, height);

        var surfaces = new Tile[checked(width * height)];
        Array.Fill(surfaces, new Tile(SurfaceType.Floor));
        _surfaceVersion = new MapSnapshot(width, height, surfaces);
    }

    /// <summary>
    /// Creates a map by copying row-major surface values into its initial snapshot.
    /// </summary>
    /// <param name="width">The number of columns.</param>
    /// <param name="height">The number of rows.</param>
    /// <param name="surfaces">
    /// One surface per cell, ordered from left to right and then top to bottom.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="surfaces"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when the number of surfaces does not match the map dimensions.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when a dimension or surface value is outside its valid range.
    /// </exception>
    public GameMap(
        int width,
        int height,
        IReadOnlyList<SurfaceType> surfaces)
    {
        ArgumentNullException.ThrowIfNull(surfaces);
        ValidateDimensions(width, height);

        int expectedCount = checked(width * height);

        if (surfaces.Count != expectedCount)
        {
            throw new ArgumentException(
                "Surface count must match the map dimensions.",
                nameof(surfaces));
        }

        var tiles = new Tile[expectedCount];

        for (int index = 0; index < expectedCount; index++)
        {
            SurfaceType surface = surfaces[index];

            if (!Enum.IsDefined(surface))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(surfaces),
                    surface,
                    $"Surface at index {index} is not a defined value.");
            }

            tiles[index] = new Tile(surface);
        }

        _surfaceVersion = new MapSnapshot(width, height, tiles);
    }

    private static void ValidateDimensions(int width, int height)
    {
        if (width is <= 0 or > MaxWidth)
        {
            throw new ArgumentOutOfRangeException(
                nameof(width),
                width,
                $"Width must be between 1 and {MaxWidth}.");
        }

        if (height is <= 0 or > MaxHeight)
        {
            throw new ArgumentOutOfRangeException(
                nameof(height),
                height,
                $"Height must be between 1 and {MaxHeight}.");
        }
    }

    /// <summary>
    /// Returns the tile stored at the specified position currently used by this map.
    /// </summary>
    /// <param name="position">An in bound position on the map.</param>
    /// <returns>The <see cref="Tile"/> stored at <paramref name="position"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="position"/> lies outside the map.
    /// </exception>
    public Tile GetSurface(Position position)
        => _surfaceVersion.GetSurface(position);

    /// <summary>
    /// Replaces the surface at the specified position with a tile of the given type currently used by this map.
    /// </summary>
    /// <param name="position">An in bound position on the map.</param>
    /// <param name="type">The surface type to store at <paramref name="position"/>.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="position"/> lies outside of the map.
    /// </exception>
    public void SetSurface(Position position, SurfaceType type)
        => _surfaceVersion = _surfaceVersion.WithSurface(position, type);

    /// <inheritdoc/>
    public bool Contains(Position position)
        => _surfaceVersion.Contains(position);

    /// <summary>
    /// Returns the immutable surface version currently used by this map.
    /// A later <see cref="SetSurface"/> call creates a new version.
    /// </summary>
    public MapSnapshot CreateSnapshot()
        => _surfaceVersion;

    /// <summary>
    /// Returns whether the specified type of movement can enter the specified position.
    /// </summary>
    /// <param name="position">The target position.</param>
    /// <param name="movementType">The type of movement attempting to do.</param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="position"/> is in bounds and its tile allows
    /// <paramref name="movementType"/>; otherwise <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// This method answers movement logic question "can I step here?" for any target. A creature at the edge of the map
    /// trying to leave the map gets <see langword="false"/> and stays in place.
    /// </remarks>
    public bool AllowsEntry(Position position, MovementType movementType)
        => _surfaceVersion.AllowsEntry(position, movementType);
}
