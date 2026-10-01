namespace RogueMaker.Core.AI;

/// <summary>The current phase of chaser brain.</summary>
public enum ChaserState
{
    /// <summary>The chaser is spending this turn preparing.</summary>
    Preparing,

    /// <summary>The chaser can choose a movement action.</summary>
    Ready,
}
