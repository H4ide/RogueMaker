using RogueMaker.Core.Creatures;
using RogueMaker.Core.Map;

namespace RogueMaker.Core.Game;

/// <summary>Identifies a creature that died during a turn and where it died.</summary>
public readonly record struct CreatureDeath(
    CreatureId CreatureId,
    Position Position);
