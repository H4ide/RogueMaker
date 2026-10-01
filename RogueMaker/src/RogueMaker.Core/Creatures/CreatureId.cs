namespace RogueMaker.Core.Creatures;

/// <summary>
/// ID of one creature during a game.
/// </summary>
public readonly record struct CreatureId
{
    /// <summary>
    /// Creates an identifier from a non-negative numeric value.
    /// </summary>
    /// <param name="value">The numeric value of the identifier.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="value"/> is negative.
    /// </exception>
    // Internal so creature IDs can only be assigned by Core, through GameState.
    internal CreatureId(int value)
    {
        if (value < 0)
            throw new ArgumentOutOfRangeException(nameof(value), value, "Creature ID cannot be negative.");

        Value = value;
    }

    public int Value { get; }

    /// <inheritdoc/>
    public override string ToString() => Value.ToString();
}
