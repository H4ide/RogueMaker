using RogueMaker.Core.Enemies;
using RogueMaker.Core.Map;

namespace RogueMaker.Core.Levels;

/// <summary>Represents mutable level data produced by the level editor.</summary>
/// <remarks>
/// A draft enforces its dimensions and prevents the player and an enemy from sharing
/// a position. The player and enemies may initially occupy any surface; movement
/// types are applied later when a creature attempts to enter another position.
/// </remarks>
public sealed class LevelDraft
{
    private readonly Grid<SurfaceType> _surfaces;
    private readonly Dictionary<Position, EnemyTypeId> _enemies = [];

    /// <summary>The number of columns in the draft.</summary>
    public int Width => _surfaces.Width;

    /// <summary>The number of rows in the draft.</summary>
    public int Height => _surfaces.Height;

    /// <summary>The current player position.</summary>
    public Position PlayerPosition { get; private set; }

    /// <summary>Enemy types indexed by their current positions.</summary>
    public IReadOnlyDictionary<Position, EnemyTypeId> Enemies => _enemies;

    /// <summary>
    /// Creates a floor-filled draft with perimeter walls and a centered player.
    /// </summary>
    /// <param name="width">The number of columns.</param>
    /// <param name="height">The number of rows.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when either dimension is outside its supported range.
    /// </exception>
    public LevelDraft(int width, int height)
    {
        if (width > GameMap.MaxWidth)
        {
            throw new ArgumentOutOfRangeException(
                nameof(width),
                width,
                $"Width cannot exceed {GameMap.MaxWidth}.");
        }

        if (height > GameMap.MaxHeight)
        {
            throw new ArgumentOutOfRangeException(
                nameof(height),
                height,
                $"Height cannot exceed {GameMap.MaxHeight}.");
        }

        _surfaces = new Grid<SurfaceType>(width, height, SurfaceType.Floor);
        AddPerimeterWalls();
        PlayerPosition = new Position(width / 2, height / 2);
    }

    /// <summary>Gets the surface stored at a position.</summary>
    /// <param name="position">An in-bounds draft position.</param>
    /// <returns>The surface stored at <paramref name="position"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="position"/> is outside the draft.
    /// </exception>
    public SurfaceType GetSurface(Position position)
        => _surfaces[position];

    /// <summary>Returns whether a position lies within the draft bounds.</summary>
    /// <param name="position">The position to test.</param>
    /// <returns>
    /// <see langword="true"/> when the position is in bounds; otherwise
    /// <see langword="false"/>.
    /// </returns>
    public bool Contains(Position position)
        => _surfaces.InBounds(position);

    /// <summary>Replaces the surface at an in-bounds position.</summary>
    /// <param name="position">The position to edit.</param>
    /// <param name="surface">The surface to store.</param>
    /// <returns>
    /// <see langword="true"/> if the surface changed; otherwise <see langword="false"/>.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="surface"/> is not a defined surface type.
    /// </exception>
    public bool TrySetSurface(Position position, SurfaceType surface)
    {
        if (!Enum.IsDefined(surface))
        {
            throw new ArgumentOutOfRangeException(
                nameof(surface),
                surface,
                "Unknown surface type.");
        }

        if (!Contains(position)
            || _surfaces[position] == surface)
        {
            return false;
        }

        _surfaces[position] = surface;
        return true;
    }

    /// <summary>Moves the player to an in-bounds position not occupied by an enemy.</summary>
    /// <param name="position">The requested player position.</param>
    /// <returns>
    /// <see langword="true"/> if the player moved; otherwise <see langword="false"/>.
    /// </returns>
    /// <remarks>The surface at the destination is intentionally not validated.</remarks>
    public bool TryMovePlayer(Position position)
    {
        if (!Contains(position)
            || position == PlayerPosition
            || _enemies.ContainsKey(position))
            return false;

        PlayerPosition = position;
        return true;
    }

    /// <summary>Adds or replaces an enemy at a position not occupied by the player.</summary>
    /// <param name="position">The requested enemy position.</param>
    /// <param name="enemyTypeId">The enemy type to store.</param>
    /// <returns>
    /// <see langword="true"/> if the enemy collection changed; otherwise
    /// <see langword="false"/>.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="enemyTypeId"/> is not a defined enemy type.
    /// </exception>
    /// <remarks>The surface at the destination is intentionally not validated.</remarks>
    public bool TrySetEnemy(Position position, EnemyTypeId enemyTypeId)
    {
        if (!Enum.IsDefined(enemyTypeId))
        {
            throw new ArgumentOutOfRangeException(
                nameof(enemyTypeId),
                enemyTypeId,
                "Unknown enemy type.");
        }

        if (!Contains(position) || position == PlayerPosition)
            return false;

        if (_enemies.TryGetValue(position, out EnemyTypeId currentType)
            && currentType == enemyTypeId)
        {
            return false;
        }

        _enemies[position] = enemyTypeId;
        return true;
    }

    /// <summary>Removes the enemy at a position, if one exists.</summary>
    /// <param name="position">The position from which to remove an enemy.</param>
    /// <returns>
    /// <see langword="true"/> if an enemy was removed; otherwise <see langword="false"/>.
    /// </returns>
    public bool TryRemoveEnemy(Position position)
        => _enemies.Remove(position);

    /// <summary>Fills the outermost rows and columns with walls. For example, if the width or height is <=2, the entire surface will be filled with walls.</summary>
    private void AddPerimeterWalls()
    {
        for (int x = 0; x < Width; x++)
        {
            _surfaces[new Position(x, 0)] = SurfaceType.Wall;
            _surfaces[new Position(x, Height - 1)] = SurfaceType.Wall;
        }

        for (int y = 1; y < Height - 1; y++)
        {
            _surfaces[new Position(0, y)] = SurfaceType.Wall;
            _surfaces[new Position(Width - 1, y)] = SurfaceType.Wall;
        }
    }
}
