namespace RogueMaker.Core.Map;

/// <summary>
/// Describes the gameplay category of surface type in a map cell.
/// </summary>
public enum SurfaceType
{
    /// <summary>regular floor, everyone can walk on it.</summary>
    Floor = 0,

    /// <summary>Blocks both walking and flying movement. Let's say it's a VERY high wall. NO ONE can stand on it. No one can climb over it. Unless you can teleport?</summary>
    Wall,

    /// <summary>An empty space that walkable creatures cannot walk over. But flying creatures can.</summary>
    Pit,

    /// <summary>Filler for the empty SurfaceTile. No one can enter it.</summary>
    Void,

    /// <summary>A walkable surface that completes the level when entered by the player.</summary>
    Exit,
}
