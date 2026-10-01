using RogueMaker.Core.Actions;
using RogueMaker.Core.Creatures;
using RogueMaker.Core.Map;

namespace RogueMaker.Core.Game;

/// <summary>Describes the resolved action of one creature.</summary>
/// <param name="From">The actor position before the action.</param>
/// <param name="Target">
/// The position the actor attempted to move into or interact with.
/// It becomes the final position only when <paramref name="Result"/> is
/// <see cref="CreatureActionResult.Moved"/>.
/// </param>
public readonly record struct CreatureActionOutcome(
    CreatureId ActorId,
    CreatureActionResult Result,
    Position From,
    Position Target);
