namespace RogueMaker.Core.Map;

/// <summary>
/// Defines a cell by its horizontal and vertical coordinates.
/// </summary>
/// <param name="X">The column, increasing from left to right.</param>
/// <param name="Y">The row, increasing from top to bottom.</param>
public readonly record struct Position(int X, int Y)
{
    /// <summary>Returns shortest distance to another position using Manhattan distance.</summary>
    public int ManhattanDistanceTo(Position other)
        => Math.Abs(X - other.X) + Math.Abs(Y - other.Y);

    /// <summary>Returns neighbour position in the specified direction.</summary>
    /// <remarks>
    /// Positions use a screen-style coordinate system where the origin is in the
    /// upper-left corner: moving right increases <see cref="X"/>, and moving
    /// down increases <see cref="Y"/>. Therefore, moving up decreases
    /// <see cref="Y"/>.
    /// </remarks>
    public static Position operator +(Position position, Direction direction)
        => direction switch
        {
            Direction.Up => new Position(position.X, position.Y - 1),
            Direction.Down => new Position(position.X, position.Y + 1),
            Direction.Left => new Position(position.X - 1, position.Y),
            Direction.Right => new Position(position.X + 1, position.Y),
            _ => throw new ArgumentOutOfRangeException(
                nameof(direction),
                direction,
                "Unknown direction."),
        };
}
