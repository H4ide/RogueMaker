namespace RogueMaker.Core.Actions;

/// <summary>Describes how a creature action changed the game world.</summary>
public enum CreatureActionResult
{
    /// <summary>The creature moved to an adjacent cell.</summary>
    Moved,

    /// <summary>The creature attacked an opponent in an adjacent cell.</summary>
    Attacked,

    /// <summary>The action was handled, but the creature remained in place.</summary>
    Blocked,

    /// <summary>The creature waited and did not perform any action.</summary>
    Waited
}
