using RogueMaker.Core.AI;
using RogueMaker.Core.Creatures;
using RogueMaker.Core.Enemies;
using RogueMaker.Core.Movement;

namespace RogueMaker.Core.Game;

/// <summary>
/// Defines the player and enemy types used by the production game.
/// </summary>
/// <remarks>
/// Saved levels contain only enemy type identifiers. This class maps those
/// identifiers to the current stats and initial brains used to start a game.
/// </remarks>
public static class GameContent
{
    /// <summary>The stats assigned to the player at the start of a game.</summary>
    public static CreatureStats PlayerStats { get; } = new(
        maxHealth: 6,
        baseDamage: 1,
        MovementType.Walking);

    /// <summary>All enemy types available in the production game.</summary>
    public static EnemyCatalog EnemyCatalog { get; } = new([
        new EnemyType(
            EnemyTypeId.Bat,
            new CreatureStats(
                maxHealth: 1,
                baseDamage: 1,
                MovementType.Flying),
            new ChaserBrain()),
        new EnemyType(
            EnemyTypeId.Slime,
            new CreatureStats(
                maxHealth: 1,
                baseDamage: 0,
                MovementType.Walking),
            WaitBrain.Instance),
        new EnemyType(
            EnemyTypeId.Skeleton,
            new CreatureStats(
                maxHealth: 3,
                baseDamage: 2,
                MovementType.Walking),
            new ChaserBrain()),
    ]);
}
