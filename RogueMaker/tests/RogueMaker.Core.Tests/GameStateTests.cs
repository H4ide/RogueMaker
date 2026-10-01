using RogueMaker.Core.Actions;
using RogueMaker.Core.Creatures;
using RogueMaker.Core.Enemies;
using RogueMaker.Core.Game;
using RogueMaker.Core.Map;
using RogueMaker.Core.Movement;

namespace RogueMaker.Core.Tests;

public class GameStateTests
{
    [Fact]
    public void Constructor_CreatesSinglePlayerWithFirstId()
    {
        var map = new GameMap(2, 2);
        var playerPosition = new Position(1, 1);
        var state = CreateState(map, playerPosition);

        Assert.Same(map, state.Map);
        Assert.Equal(0, state.Player.Id.Value);
        Assert.Same(state.Player, state.GetCreatureAt(playerPosition));
        Assert.Null(state.GetCreatureAt(new Position(0, 0)));
        Assert.Empty(state.Enemies);
    }

    [Fact]
    public void AddEnemy_AssignsUniqueSequentialIdsAndStoresEnemies()
    {
        var enemyType = new EnemyType(
            EnemyTypeId.Bat,
            new CreatureStats(3, 1, MovementType.Flying));
        var state = CreateState(new GameMap(2, 2), new Position(0, 0), enemyType);

        var firstPosition = new Position(0, 1);
        var secondPosition = new Position(1, 1);
        state.AddEnemy(firstPosition, enemyType.TypeId);
        state.AddEnemy(secondPosition, enemyType.TypeId);

        Enemy first = Assert.Single(state.Enemies, enemy => enemy.Position == firstPosition);
        Enemy second = Assert.Single(state.Enemies, enemy => enemy.Position == secondPosition);

        Assert.Equal(1, first.Id.Value);
        Assert.Equal(2, second.Id.Value);
        Assert.NotEqual(first.Id, second.Id);
        Assert.Same(first.EnemyType, second.EnemyType);
        Assert.Same(first.Stats, second.Stats);
        Assert.Equal(2, state.Enemies.Count);
        Assert.Contains(first, state.Enemies);
        Assert.Contains(second, state.Enemies);
    }

    [Theory]
    [InlineData(SurfaceType.Exit, MovementType.Walking)]
    [InlineData(SurfaceType.Exit, MovementType.Flying)]
    [InlineData(SurfaceType.Wall, MovementType.Walking)]
    [InlineData(SurfaceType.Wall, MovementType.Flying)]
    [InlineData(SurfaceType.Pit, MovementType.Walking)]
    [InlineData(SurfaceType.Pit, MovementType.Flying)]
    public void AddEnemy_AnySurface_AddsEnemy(
        SurfaceType surface,
        MovementType movementType)
    {
        var map = new GameMap(2, 1);
        var enemyPosition = new Position(1, 0);
        map.SetSurface(enemyPosition, surface);
        EnemyType enemyType = CreateEnemyType(movementType);
        var state = CreateState(map, new Position(0, 0), enemyType);

        state.AddEnemy(enemyPosition, enemyType.TypeId);

        Enemy enemy = Assert.Single(state.Enemies);
        Assert.Equal(enemyPosition, enemy.Position);
        Assert.Same(enemy, state.GetCreatureAt(enemyPosition));
    }

    [Fact]
    public void AddEnemy_ToPlayerPosition_ThrowsInvalidOperationException()
    {
        EnemyType enemyType = CreateEnemyType(MovementType.Walking);
        var state = CreateState(new GameMap(2, 1), new Position(0, 0), enemyType);

        Assert.Throws<InvalidOperationException>(
            () => state.AddEnemy(new Position(0, 0), enemyType.TypeId));
    }

    [Fact]
    public void AddEnemy_ToAnotherEnemyPosition_ThrowsInvalidOperationException()
    {
        var enemyPosition = new Position(1, 0);
        EnemyType enemyType = CreateEnemyType(MovementType.Walking);
        var state = CreateState(new GameMap(3, 1), new Position(0, 0), enemyType);
        state.AddEnemy(enemyPosition, enemyType.TypeId);

        Assert.Throws<InvalidOperationException>(() => state.AddEnemy(enemyPosition, enemyType.TypeId));
        Assert.Single(state.Enemies);
    }

    [Fact]
    public void AddEnemy_FailedPlacementDoesNotConsumeId()
    {
        var map = new GameMap(2, 1);
        var outsideMap = new Position(-1, 0);
        var enemyPosition = new Position(1, 0);
        EnemyType enemyType = CreateEnemyType(MovementType.Walking);
        var state = CreateState(map, new Position(0, 0), enemyType);

        Assert.Throws<ArgumentException>(
            () => state.AddEnemy(outsideMap, enemyType.TypeId));

        state.AddEnemy(enemyPosition, enemyType.TypeId);

        Assert.Equal(1, Assert.Single(state.Enemies).Id.Value);
    }

    [Theory]
    [InlineData(-1, 0)]
    [InlineData(2, 0)]
    public void AddEnemy_RejectsPositionOutsideMap(int x, int y)
    {
        EnemyType enemyType = CreateEnemyType(MovementType.Flying);
        var state = CreateState(new GameMap(2, 1), new Position(0, 0), enemyType);

        Assert.Throws<ArgumentException>(
            () => state.AddEnemy(new Position(x, y), enemyType.TypeId));
    }

    [Theory]
    [InlineData(SurfaceType.Wall)]
    [InlineData(SurfaceType.Pit)]
    public void Constructor_PlayerCanStartOnAnySurface(SurfaceType surface)
    {
        var map = new GameMap(1, 1);
        var playerPosition = new Position(0, 0);
        map.SetSurface(playerPosition, surface);

        GameState state = CreateState(map, playerPosition);

        Assert.Equal(playerPosition, state.Player.Position);
        Assert.Same(state.Player, state.GetCreatureAt(playerPosition));
    }

    [Theory]
    [InlineData(-1, 0)]
    [InlineData(1, 0)]
    public void Constructor_RejectsPlayerPositionOutsideMap(int x, int y)
    {
        Assert.Throws<ArgumentException>(
            () => CreateState(new GameMap(1, 1), new Position(x, y)));
    }

    [Fact]
    public void TryAddEnemy_ToAvailablePosition_AddsEnemyAndReturnsTrue()
    {
        var enemyPosition = new Position(1, 0);
        EnemyType enemyType = CreateEnemyType(MovementType.Walking);
        var state = CreateState(new GameMap(2, 1), new Position(0, 0), enemyType);

        bool added = state.TryAddEnemy(enemyPosition, enemyType.TypeId);

        Assert.True(added);
        Assert.Same(Assert.Single(state.Enemies), state.GetCreatureAt(enemyPosition));
    }

    [Theory]
    [InlineData(SurfaceType.Wall)]
    [InlineData(SurfaceType.Pit)]
    public void TryAddEnemy_AnySurface_AddsEnemy(SurfaceType surface)
    {
        var map = new GameMap(2, 1);
        var enemyPosition = new Position(1, 0);
        map.SetSurface(enemyPosition, surface);
        EnemyType enemyType = CreateEnemyType(MovementType.Walking);
        var state = CreateState(map, new Position(0, 0), enemyType);

        bool added = state.TryAddEnemy(enemyPosition, enemyType.TypeId);

        Assert.True(added);
        Assert.Same(Assert.Single(state.Enemies), state.GetCreatureAt(enemyPosition));
    }

    [Fact]
    public void TryAddEnemy_OutsideMap_ReturnsFalseWithoutConsumingId()
    {
        var map = new GameMap(2, 1);
        var outsideMap = new Position(-1, 0);
        var availablePosition = new Position(1, 0);
        EnemyType enemyType = CreateEnemyType(MovementType.Walking);
        var state = CreateState(map, new Position(0, 0), enemyType);

        bool addedOutsideMap = state.TryAddEnemy(outsideMap, enemyType.TypeId);
        bool addedToAvailablePosition = state.TryAddEnemy(availablePosition, enemyType.TypeId);

        Assert.False(addedOutsideMap);
        Assert.True(addedToAvailablePosition);
        Enemy enemy = Assert.Single(state.Enemies);
        Assert.Equal(1, enemy.Id.Value);
        Assert.Same(enemy, state.GetCreatureAt(availablePosition));
    }

    [Fact]
    public void TryAddEnemy_ToOccupiedPosition_ReturnsFalse()
    {
        EnemyType enemyType = CreateEnemyType(MovementType.Walking);
        var playerPosition = new Position(0, 0);
        var state = CreateState(new GameMap(1, 1), playerPosition, enemyType);

        bool added = state.TryAddEnemy(playerPosition, enemyType.TypeId);

        Assert.False(added);
        Assert.Empty(state.Enemies);
        Assert.Same(state.Player, state.GetCreatureAt(playerPosition));
    }

    [Fact]
    public void TryMoveCreature_MovesPlayerAndUpdatesOccupancy()
    {
        var start = new Position(0, 0);
        var target = new Position(1, 0);
        var state = CreateState(new GameMap(2, 1), start);

        bool moved = state.TryMoveCreature(state.Player.Id, Direction.Right);

        Assert.True(moved);
        Assert.Equal(target, state.Player.Position);
        Assert.Null(state.GetCreatureAt(start));
        Assert.Same(state.Player, state.GetCreatureAt(target));
    }

    [Theory]
    [InlineData(SurfaceType.Wall)]
    [InlineData(SurfaceType.Pit)]
    public void TryMoveCreature_PlayerStartingOnBlockedSurface_MovesToAllowedSurface(
        SurfaceType startSurface)
    {
        var start = new Position(0, 0);
        var target = new Position(1, 0);
        var map = new GameMap(2, 1);
        map.SetSurface(start, startSurface);
        var state = CreateState(map, start);

        bool moved = state.TryMoveCreature(state.Player.Id, Direction.Right);

        Assert.True(moved);
        Assert.Equal(target, state.Player.Position);
        Assert.Null(state.GetCreatureAt(start));
        Assert.Same(state.Player, state.GetCreatureAt(target));
    }

    [Fact]
    public void TryMoveCreature_MovesEnemyResolvedById()
    {
        EnemyType enemyType = CreateEnemyType(MovementType.Walking);
        var state = CreateState(new GameMap(3, 1), new Position(0, 0), enemyType);
        state.AddEnemy(new Position(1, 0), enemyType.TypeId);
        Enemy enemy = Assert.Single(state.Enemies);

        bool moved = state.TryMoveCreature(enemy.Id, Direction.Right);

        Assert.True(moved);
        Assert.Equal(new Position(2, 0), enemy.Position);
        Assert.Same(enemy, state.GetCreatureAt(new Position(2, 0)));
    }

    [Fact]
    public void TryMoveCreature_ToOccupiedPosition_ReturnsFalseAndDoesNotMove()
    {
        var playerPosition = new Position(0, 0);
        var enemyPosition = new Position(1, 0);
        EnemyType enemyType = CreateEnemyType(MovementType.Walking);
        var state = CreateState(new GameMap(2, 1), playerPosition, enemyType);
        state.AddEnemy(enemyPosition, enemyType.TypeId);

        bool moved = state.TryMoveCreature(state.Player.Id, Direction.Right);

        Assert.False(moved);
        Assert.Equal(playerPosition, state.Player.Position);
        Assert.Same(state.Player, state.GetCreatureAt(playerPosition));
        Assert.Same(Assert.Single(state.Enemies), state.GetCreatureAt(enemyPosition));
    }

    [Theory]
    [InlineData(SurfaceType.Wall)]
    [InlineData(SurfaceType.Pit)]
    public void TryMoveCreature_ToSurfaceThatBlocksWalking_ReturnsFalse(SurfaceType surface)
    {
        var start = new Position(0, 0);
        var target = new Position(1, 0);
        var map = new GameMap(2, 1);
        map.SetSurface(target, surface);
        var state = CreateState(map, start);

        bool moved = state.TryMoveCreature(state.Player.Id, Direction.Right);

        Assert.False(moved);
        Assert.Equal(start, state.Player.Position);
        Assert.Same(state.Player, state.GetCreatureAt(start));
        Assert.Null(state.GetCreatureAt(target));
    }

    [Fact]
    public void TryMoveCreature_OutsideMap_ReturnsFalseAndDoesNotMove()
    {
        var start = new Position(0, 0);
        var state = CreateState(new GameMap(1, 1), start);

        bool moved = state.TryMoveCreature(state.Player.Id, Direction.Left);

        Assert.False(moved);
        Assert.Equal(start, state.Player.Position);
        Assert.Same(state.Player, state.GetCreatureAt(start));
    }

    [Fact]
    public void RemoveEnemy_RemovesEnemyAndFreesItsPosition()
    {
        var enemyPosition = new Position(1, 0);
        EnemyType enemyType = CreateEnemyType(MovementType.Walking);
        var state = CreateState(new GameMap(2, 1), new Position(0, 0), enemyType);
        state.AddEnemy(enemyPosition, enemyType.TypeId);
        Enemy enemy = Assert.Single(state.Enemies);

        bool removed = state.RemoveEnemy(enemy.Id);

        Assert.True(removed);
        Assert.Empty(state.Enemies);
        Assert.Null(state.GetCreatureAt(enemyPosition));
        Assert.True(state.TryMoveCreature(state.Player.Id, Direction.Right));
    }

    [Fact]
    public void TryAttackCreature_OpposingCreature_DealsDamageAndReturnsResult()
    {
        var enemyPosition = new Position(1, 0);
        EnemyType enemyType = CreateEnemyType(MovementType.Walking);
        var state = CreateState(new GameMap(2, 1), new Position(0, 0), enemyType);
        state.AddEnemy(enemyPosition, enemyType.TypeId);
        Enemy enemy = Assert.Single(state.Enemies);

        bool attacked = state.TryAttackCreature(
            state.Player.Id,
            Direction.Right,
            out AttackResult result);

        Assert.True(attacked);
        Assert.Equal(state.Player.Id, result.AttackerId);
        Assert.Equal(enemy.Id, result.TargetId);
        Assert.Equal(2, result.Damage);
        Assert.False(result.TargetDied);
        Assert.Equal(1, enemy.Health);
        Assert.Same(enemy, state.GetCreatureAt(enemyPosition));
    }

    [Fact]
    public void TryAttackCreature_WithoutTarget_ReturnsFalse()
    {
        var state = CreateState(new GameMap(2, 1), new Position(0, 0));

        bool attacked = state.TryAttackCreature(
            state.Player.Id,
            Direction.Right,
            out AttackResult result);

        Assert.False(attacked);
        Assert.Equal(default, result);
    }

    [Fact]
    public void TryAttackCreature_CreatureOnSameSide_ReturnsFalse()
    {
        EnemyType enemyType = CreateEnemyType(MovementType.Walking);
        var state = CreateState(new GameMap(3, 1), new Position(0, 0), enemyType);
        state.AddEnemy(new Position(1, 0), enemyType.TypeId);
        state.AddEnemy(new Position(2, 0), enemyType.TypeId);
        Enemy attacker = Assert.Single(state.Enemies, enemy => enemy.Position == new Position(1, 0));
        Enemy target = Assert.Single(state.Enemies, enemy => enemy.Position == new Position(2, 0));

        bool attacked = state.TryAttackCreature(
            attacker.Id,
            Direction.Right,
            out AttackResult result);

        Assert.False(attacked);
        Assert.Equal(default, result);
        Assert.Equal(target.Stats.MaxHealth, target.Health);
    }

    [Fact]
    public void TryAttackCreature_LethalDamageRemovesEnemyAndReportsDeath()
    {
        var enemyType = new EnemyType(
            EnemyTypeId.Bat,
            new CreatureStats(2, 1, MovementType.Walking));
        var enemyPosition = new Position(1, 0);
        var state = CreateState(new GameMap(2, 1), new Position(0, 0), enemyType);
        state.AddEnemy(enemyPosition, enemyType.TypeId);
        Enemy enemy = Assert.Single(state.Enemies);

        bool attacked = state.TryAttackCreature(
            state.Player.Id,
            Direction.Right,
            out AttackResult result);

        Assert.True(attacked);
        Assert.Equal(enemy.Id, result.TargetId);
        Assert.Equal(2, result.Damage);
        Assert.True(result.TargetDied);
        Assert.False(enemy.IsAlive);
        Assert.Empty(state.Enemies);
        Assert.Null(state.GetCreatureAt(enemyPosition));
    }

    [Fact]
    public void ApplyPlayerAction_MoveToEmptyCell_MovesPlayer()
    {
        var state = CreateState(new GameMap(2, 1), new Position(0, 0));

        CreatureActionOutcome outcome = state.ApplyPlayerAction(
            Direction.Right).Outcome;

        Assert.Equal(state.Player.Id, outcome.ActorId);
        Assert.Equal(CreatureActionResult.Moved, outcome.Result);
        Assert.Equal(new Position(1, 0), state.Player.Position);
    }

    [Fact]
    public void ApplyPlayerAction_MoveTowardsEnemy_AttacksWithoutMoving()
    {
        var playerPosition = new Position(0, 0);
        var enemyPosition = new Position(1, 0);
        EnemyType enemyType = CreateEnemyType(MovementType.Walking);
        var state = CreateState(new GameMap(2, 1), playerPosition, enemyType);
        state.AddEnemy(enemyPosition, enemyType.TypeId);
        Enemy enemy = Assert.Single(state.Enemies);

        CreatureActionOutcome outcome = state.ApplyPlayerAction(
            Direction.Right).Outcome;

        Assert.Equal(state.Player.Id, outcome.ActorId);
        Assert.Equal(CreatureActionResult.Attacked, outcome.Result);
        Assert.Equal(playerPosition, outcome.From);
        Assert.Equal(enemyPosition, outcome.Target);
        Assert.Equal(playerPosition, state.Player.Position);
        Assert.Equal(1, enemy.Health);
    }

    [Fact]
    public void ApplyPlayerAction_MoveIntoWall_ReturnsBlockedWithoutMoving()
    {
        var playerPosition = new Position(0, 0);
        var map = new GameMap(2, 1);
        map.SetSurface(new Position(1, 0), SurfaceType.Wall);
        var state = CreateState(map, playerPosition);

        CreatureActionOutcome outcome = state.ApplyPlayerAction(
            Direction.Right).Outcome;

        Assert.Equal(state.Player.Id, outcome.ActorId);
        Assert.Equal(CreatureActionResult.Blocked, outcome.Result);
        Assert.Equal(playerPosition, state.Player.Position);
    }

    [Fact]
    public void ApplyPlayerAction_MoveOutsideMap_ReturnsBlockedWithoutMoving()
    {
        var playerPosition = new Position(0, 0);
        var state = CreateState(new GameMap(1, 1), playerPosition);

        CreatureActionOutcome outcome = state.ApplyPlayerAction(
            Direction.Left).Outcome;

        Assert.Equal(state.Player.Id, outcome.ActorId);
        Assert.Equal(CreatureActionResult.Blocked, outcome.Result);
        Assert.Equal(playerPosition, state.Player.Position);
    }

    [Fact]
    public void ApplyPlayerAction_WhenPlayerIsDead_ReturnsBlockedWithoutMoving()
    {
        var enemyType = new EnemyType(
            EnemyTypeId.Bat,
            new CreatureStats(3, 10, MovementType.Walking));
        var playerPosition = new Position(1, 0);
        var state = CreateState(new GameMap(3, 1), playerPosition, enemyType);
        state.AddEnemy(new Position(2, 0), enemyType.TypeId);
        Enemy enemy = Assert.Single(state.Enemies);
        Assert.True(state.TryAttackCreature(enemy.Id, Direction.Left, out _));

        CreatureActionOutcome outcome = state.ApplyPlayerAction(
            Direction.Left).Outcome;

        Assert.False(state.Player.IsAlive);
        Assert.Equal(state.Player.Id, outcome.ActorId);
        Assert.Equal(CreatureActionResult.Blocked, outcome.Result);
        Assert.Equal(playerPosition, state.Player.Position);
    }

    [Fact]
    public void ApplyPlayerAction_LethalAttackRemovesEnemy()
    {
        var enemyType = new EnemyType(
            EnemyTypeId.Bat,
            new CreatureStats(2, 1, MovementType.Walking));
        var enemyPosition = new Position(1, 0);
        var state = CreateState(new GameMap(2, 1), new Position(0, 0), enemyType);
        state.AddEnemy(enemyPosition, enemyType.TypeId);
        Enemy enemy = Assert.Single(state.Enemies);

        CreatureActionOutcome outcome = state.ApplyPlayerAction(
            Direction.Right).Outcome;

        Assert.Equal(state.Player.Id, outcome.ActorId);
        Assert.Equal(CreatureActionResult.Attacked, outcome.Result);
        Assert.Equal(enemyPosition, outcome.Target);
        Assert.False(enemy.IsAlive);
        Assert.Empty(state.Enemies);
        Assert.Null(state.GetCreatureAt(enemyPosition));
    }

    private static CreatureStats PlayerStats()
        => new(10, 2, MovementType.Walking);

    private static EnemyType CreateEnemyType(MovementType movementType)
        => new EnemyType(
            EnemyTypeId.Bat,
            new CreatureStats(3, 1, movementType));

    private static GameState CreateState(
        GameMap map,
        Position playerPosition,
        params EnemyType[] enemyTypes)
        => new(
            map,
            new EnemyCatalog(enemyTypes),
            playerPosition,
            PlayerStats());
}
