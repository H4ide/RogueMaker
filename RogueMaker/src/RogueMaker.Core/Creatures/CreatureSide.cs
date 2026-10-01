namespace RogueMaker.Core.Creatures;

/// <summary>
/// Describes which side controls a creature.
/// </summary>
public enum CreatureSide
{
    /// <summary>The single creature controlled by the player.</summary>
    Player,

    /// <summary>A creature controlled by the game.</summary>
    Enemy,
}
