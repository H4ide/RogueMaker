using RogueMaker.Core.Creatures;
using RogueMaker.Core.Enemies;
using RogueMaker.Core.Movement;

namespace RogueMaker.Core.Tests;

public class EnemyCatalogTests
{
    [Fact]
    public void Catalog_WorkingExample_NoError()
    {
        EnemyType bat = CreateDefinition(EnemyTypeId.Bat, maxHealth: 2);
        EnemyType slime = CreateDefinition(EnemyTypeId.Slime, maxHealth: 3);
        var catalog = new EnemyCatalog([bat, slime]);

        Assert.Equal(2, catalog.Count);
        Assert.Contains(bat, catalog);
        Assert.Contains(slime, catalog);
        Assert.Same(bat, catalog.Get(EnemyTypeId.Bat));
    }

    [Fact]
    public void Catalog_RejectsDuplicateTypeIds()
    {
        // MovementType = Walking, baseDamage = 1
        EnemyType weakBat = CreateDefinition(EnemyTypeId.Bat, maxHealth: 2);
        EnemyType strongBat = CreateDefinition(EnemyTypeId.Bat, maxHealth: 20);

        Assert.Throws<ArgumentException>(() => new EnemyCatalog([weakBat, strongBat]));
    }

    [Fact]
    public void Get_ThrowsForUnknownType()
    {
        var catalog = new EnemyCatalog([]);

        Assert.Throws<KeyNotFoundException>(() => catalog.Get(EnemyTypeId.Slime));
    }

    private static EnemyType CreateDefinition(EnemyTypeId typeId, int maxHealth)
        => new EnemyType(
            typeId,
            new CreatureStats(maxHealth, baseDamage: 1, MovementType.Walking));
}
