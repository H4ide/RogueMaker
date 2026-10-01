namespace RogueMaker.Core.Game;

/// <summary>Describes whether a running game can continue.</summary>
public enum GameStatus
{
    /// <summary>The player can perform another action.</summary>
    InProgress,

    /// <summary>The player completed a victory condition.</summary>
    Won,

    /// <summary>The player died.</summary>
    Lost,
}
