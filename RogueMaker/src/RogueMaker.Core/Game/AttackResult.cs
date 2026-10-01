using RogueMaker.Core.Creatures;

namespace RogueMaker.Core.Game;

/// <summary>Describes a successfully resolved creature attack.</summary>
public readonly record struct AttackResult(
    CreatureId AttackerId,
    CreatureId TargetId,
    int Damage,
    bool TargetDied);
