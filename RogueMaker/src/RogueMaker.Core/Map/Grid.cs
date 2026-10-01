namespace RogueMaker.Core.Map;

/// <summary>
/// Stores values of type <typeparamref name="T"/> in a fixed-size rectangular grid.
/// !!! NOT typical matrix, the first index is x (column), the second index is y (row).
/// Forth quadrant of oxy plane, the origin is at the top left corner, x increases to the right, y increases downwards.
/// public methods works with Position struct, which is a pair of (x, y) coordinates. Private methods works with x and y coordinates separately.
/// </summary>
/// <typeparam name="T">The type of value stored in each cell.</typeparam>
public sealed class Grid<T>
{   /// <summary>Stores the grid as a 1D array, and compute the index of each cell from its coordinates.</summary>
    private readonly T[] _cells;

    /// <summary>The number of columns in the grid.</summary>
    public int Width { get; }

    /// <summary>The number of rows in the grid.</summary>
    public int Height { get; }

    /// <summary>
    /// Creates a grid with cells containing the default value of <typeparamref name="T"/>.
    /// </summary>
    public Grid(int width, int height)
        : this(width, height, default!)
    {
    }

    /// <summary>
    /// Creates a grid and fills every cell with <paramref name="initialValue"/>.
    /// <paramref name="height"/>  is the number of rows, 
    /// and <paramref name="width"/> is the number of columns should be positive integers, 
    /// otherwise an <see cref="ArgumentOutOfRangeException"/> is thrown.
    /// </summary>
    public Grid(int width, int height, T initialValue)
    {
        if (width <= 0)
            throw new ArgumentOutOfRangeException(nameof(width), "Width must be positive.");

        if (height <= 0)
            throw new ArgumentOutOfRangeException(nameof(height), "Height must be positive.");

        Width = width;
        Height = height;
        // Allocates a 1D array to store the grid cells, and fills it with the initial value.
        _cells = new T[width * height];
        Array.Fill(_cells, initialValue);
    }

    /// <summary>Gets or replaces the value stored at the specified coordinates.</summary>
    private T this[int x, int y]
    {
        get => _cells[GetIndex(x, y)];
        set => _cells[GetIndex(x, y)] = value;
    }

    /// <summary>Gets or replaces the value stored at the specified <see cref="Position"/>.</summary>
    public T this[Position position]
    {
        get => this[position.X, position.Y];
        set => this[position.X, position.Y] = value;
    }

    /// <summary>
    /// Returns whether the specified coordinates are within the bounds of the grid.
    /// </summary>
    /// <param name="x">The x-coordinate of the cell.</param>
    /// <param name="y">The y-coordinate of the cell.</param>
    /// <returns>True if the coordinates are within the bounds of the grid; otherwise, false.</returns>
    private bool InBounds(int x, int y)
        => x >= 0 && x < Width && y >= 0 && y < Height;

    /// <summary>Returns whether the specified position is within the bounds of the grid.</summary>
    public bool InBounds(Position position) => InBounds(position.X, position.Y);

    /// <summary>
    /// Returns the linear index of the cell in the internal array. First coordinate is left to right, second coordinate is top to bottom.
    /// </summary>
    /// <param name="x">The x-coordinate of the cell.</param>
    /// <param name="y">The y-coordinate of the cell.</param>
    /// <returns>The linear index of the cell in the internal array.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the coordinates are out of bounds.</exception>
    private int GetIndex(int x, int y)
    {
        if (!InBounds(x, y))
        {
            throw new ArgumentOutOfRangeException(
                $"({x}, {y})",
                $"Coordinates must be inside a Width:{Width}xHeight:{Height} grid.");
        }

        return (y * Width) + x;
    }
}
