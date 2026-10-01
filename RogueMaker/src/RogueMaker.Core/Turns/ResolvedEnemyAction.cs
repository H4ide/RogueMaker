using RogueMaker.Core.Actions;
using RogueMaker.Core.Map;

namespace RogueMaker.Core.Turns;

/// <summary>One planned action after movement conflicts and dependencies are resolved.</summary>
public readonly record struct ResolvedEnemyAction(
    PlannedEnemyAction Plan,
    CreatureActionResult Result,
    Position From,
    Position Target);
