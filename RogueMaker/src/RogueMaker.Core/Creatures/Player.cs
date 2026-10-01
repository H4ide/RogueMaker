using RogueMaker.Core.Map;

namespace RogueMaker.Core.Creatures;

/// <summary>
/// The single creature controlled by the player.
/// </summary>
public class Player : Creature
{
    /// <summary>Creates a player at the specified position.</summary>
    // Internal so player is created and assigned an ID only through GameState (so,only inside Core).
    internal Player(CreatureId id, Position position, CreatureStats stats)
        : base(id, position, stats)
    {
    }

    /// <inheritdoc/>
    public override CreatureSide Side => CreatureSide.Player;
}
