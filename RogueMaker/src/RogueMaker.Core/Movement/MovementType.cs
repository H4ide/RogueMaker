namespace RogueMaker.Core.Movement;

/// <summary>
/// Describes how an actor moves across the map.
/// </summary>
public enum MovementType
{
    /// <summary>The actor moves by walking on a surface.</summary>
    Walking,

    /// <summary>The actor moves by flying over a surface.</summary>
    Flying,
}
