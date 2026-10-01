using RogueMaker.Core.Actions;
using RogueMaker.Core.Creatures;

namespace RogueMaker.Core.Turns;

/// <summary>An action chosen by an enemy but not yet applied to the world.</summary>
public readonly record struct PlannedEnemyAction(
    CreatureId EnemyId,
    EnemyAction Action);
