namespace RogueMaker.Core.Enemies;

/// <summary>
/// Identifies an enemy type supported by this game version.
/// </summary>
/// <remarks>
/// An enum is used because the game will have at max couple tens of enemies even with new updates.
/// It prevents null or arbitrary string IDs; can add new types. 
/// While struct or class IDs would be more flexible, they would also be more error-prone and less efficient.
/// </remarks>
public enum EnemyTypeId
{
    /// <summary>A bat enemy.</summary>
    Bat = 1,

    /// <summary>A slime enemy.</summary>
    Slime = 2,

    /// <summary>A skeleton enemy.</summary>
    Skeleton = 3,
}
