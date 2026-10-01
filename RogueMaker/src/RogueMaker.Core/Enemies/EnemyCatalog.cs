using System.Collections;

namespace RogueMaker.Core.Enemies;

/// <summary>
/// Stores the enemy types registered for one game version.
/// </summary>
public sealed class EnemyCatalog : IReadOnlyCollection<EnemyType>
{
    private readonly Dictionary<EnemyTypeId, EnemyType> _types;

    /// <summary>
    /// Creates a catalog from a collection of types with unique type identifiers.
    /// </summary>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="types"/> or one of its elements is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when multiple types use the same enemy type identifier.
    /// </exception>
    public EnemyCatalog(IEnumerable<EnemyType> types)
    {
        ArgumentNullException.ThrowIfNull(types);

        _types = new Dictionary<EnemyTypeId, EnemyType>();
        foreach (EnemyType type in types)
        {
            ArgumentNullException.ThrowIfNull(type);

            if (!_types.TryAdd(type.TypeId, type))
            {
                throw new ArgumentException(
                    $"Enemy type '{type.TypeId}' is registered more than once.",
                    nameof(types));
            }
        }
    }

    /// <inheritdoc/>
    public int Count => _types.Count;

    /// <inheritdoc/>
    public EnemyType Get(EnemyTypeId typeId)
    {
        if (_types.TryGetValue(typeId, out EnemyType? _typeId))
            return _typeId;

        throw new KeyNotFoundException($"Enemy type '{typeId}' is not registered.");
    }

    /// <inheritdoc/>
    public IEnumerator<EnemyType> GetEnumerator()
        => _types.Values.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
