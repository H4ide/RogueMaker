using RogueMaker.Core.Creatures;
using RogueMaker.Core.Enemies;
using RogueMaker.Core.Movement;

namespace RogueMaker.Core.Tests;

public class EnemyTypeTests
{
    [Fact]
    public void CreatureStats_SimpleInitialization_NoError()
    {
        var stats = new CreatureStats(
            maxHealth: 3,
            baseDamage: 1,
            movementType: MovementType.Flying);

        Assert.Equal(3, stats.MaxHealth);
        Assert.Equal(1, stats.BaseDamage);
        Assert.Equal(MovementType.Flying, stats.MovementType);
    }

    [Fact]
    public void EnemyType_SimpleInit_NoError()
    {
        var bat = new EnemyType(
            EnemyTypeId.Bat,
            new CreatureStats(
                maxHealth: 3,
                baseDamage: 1,
                movementType: MovementType.Flying));
        Assert.Equal(3, bat.Stats.MaxHealth);
        Assert.Equal(1, bat.Stats.BaseDamage);
        Assert.Equal(MovementType.Flying, bat.Stats.MovementType);
    }

    [Fact]
    public void EnemyType_RejectsNullStats()
    {
        Assert.Throws<ArgumentNullException>(
            () => new EnemyType(EnemyTypeId.Bat, null!));
    }
}
