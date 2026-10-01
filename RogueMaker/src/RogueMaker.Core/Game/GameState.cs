using RogueMaker.Core.Actions;
using RogueMaker.Core.Creatures;
using RogueMaker.Core.Enemies;
using RogueMaker.Core.Map;
using RogueMaker.Core.Movement;

namespace RogueMaker.Core.Game;

/// <summary>
/// Owns the map and the creatures that belong to one running game.
/// Keeps creature placement inside the current map and prevents two creatures
/// from occupying the same position.
/// </summary>
/// <remarks>
/// <para>
/// Initial placement may use any surface. Surface traversal is validated against
/// each creature's movement type only when that creature attempts to move.
/// </para>
/// <para>
/// Keep it stupid simple. I did not like the fact that both the creature and the world state store the creature's position.
/// I spent a non-trivial amount of time trying to solve this problem, for example, by removing the current position from the creature itself.
/// However, in that case I would need to maintain two identical lookup structures: 
/// one from position to ID and another from ID to position.That would still duplicate data.
/// So I decided to keep a small amount of duplicated data: 
/// one dictionary for all creatures and another dictionary specifically for enemies.
/// The first one lets me quickly check whether a cell is occupied, 
/// while the second one lets each enemy take its turn without accidentally including the player.
/// </para>
/// </remarks>
public class GameState
{
    private readonly Dictionary<CreatureId, Creature> _creatures = [];
    private readonly Dictionary<CreatureId, Enemy> _enemies = [];
    private readonly CreatureOccupancy _occupancy = new();
    private readonly EnemyCatalog _enemyCatalog;
    private int _nextCreatureId;

    /// <summary>Creates a game with its single player at any in-bounds surface.</summary>
    public GameState(
        GameMap map,
        EnemyCatalog enemyCatalog,
        Position playerPosition,
        CreatureStats playerStats)
    {
        ArgumentNullException.ThrowIfNull(map);
        ArgumentNullException.ThrowIfNull(enemyCatalog);
        ArgumentNullException.ThrowIfNull(playerStats);

        Map = map;
        _enemyCatalog = enemyCatalog;
        EnsureCanPlace(playerPosition);
        Player = new Player(NextCreatureId(), playerPosition, playerStats);
        _occupancy.Place(Player.Position, Player.Id);
        _creatures.Add(Player.Id, Player);
    }

    /// <summary>The map used by this game.</summary>
    public GameMap Map { get; }

    /// <summary>The single player creature.</summary>
    public Player Player { get; }

    /// <summary>All enemies currently belonging to this game.</summary>
    public IReadOnlyCollection<Enemy> Enemies => _enemies.Values;

    /// <summary>Captures the current map version and creature values in an immutable snapshot.</summary>
    public WorldSnapshot CreateSnapshot()
        => WorldSnapshot.Capture(
            Map.CreateSnapshot(),
            _creatures.Values);

    /// <summary>Returns the enemy with the specified ID, or <see langword="null"/>.</summary>
    internal Enemy? GetEnemy(CreatureId enemyId)
    {
        _enemies.TryGetValue(enemyId, out Enemy? enemy);
        return enemy;
    }

    /// <summary>Returns the creature at a position, or <see langword="null"/> if it is empty.</summary>
    public Creature? GetCreatureAt(Position position)
    {
        CreatureId? occupantId = _occupancy.GetOccupant(position);

        if (occupantId is null)
            return null;

        return _creatures[occupantId.Value];
    }

    /// <summary>Creates and adds an enemy at any unoccupied in-bounds surface.</summary>
    public void AddEnemy(Position position, EnemyTypeId enemyTypeId)
    {
        EnemyType enemyType = _enemyCatalog.Get(enemyTypeId);
        EnsureCanPlace(position);
        AddEnemy(position, enemyType);
    }

    /// <summary>
    /// Attempts to create and add an enemy at any unoccupied in-bounds surface.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> if the enemy was added; otherwise <see langword="false"/>.
    /// </returns>
    public bool TryAddEnemy(Position position, EnemyTypeId enemyTypeId)
    {
        EnemyType enemyType = _enemyCatalog.Get(enemyTypeId);

        if (!CanPlace(position))
            return false;

        AddEnemy(position, enemyType);
        return true;
    }

    /// <summary>
    /// Attempts to move a creature one cell onto a surface allowed by its movement type.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> if the creature moved; otherwise <see langword="false"/>.
    /// </returns>
    public bool TryMoveCreature(CreatureId creatureId, Direction direction)
    {
        if (!_creatures.TryGetValue(creatureId, out Creature? creature) || !creature.IsAlive)
            return false;

        Position target = creature.Position + direction;

        if (!MovementQueries.CanEnter(creature, Map, target)
            || _occupancy.IsOccupied(target))
            return false;

        _occupancy.Move(creature.Position, target, creature.Id);
        creature.SetPosition(target);
        return true;
    }

    /// <summary>Attempts to attack an opposing creature in an adjacent cell.</summary>
    /// <returns>
    /// <see langword="true"/> if an opposing creature was attacked;
    /// otherwise <see langword="false"/>.
    /// </returns>
    public bool TryAttackCreature(
        CreatureId attackerId,
        Direction direction,
        out AttackResult result)
    {
        result = default;

        if (!_creatures.TryGetValue(attackerId, out Creature? attacker) || !attacker.IsAlive)
            return false;

        Creature? target = GetCreatureAt(attacker.Position + direction);

        if (target is null || !target.IsAlive || target.Side == attacker.Side)
            return false;

        int healthBeforeAttack = target.Health;
        target.TakeDamage(attacker.Stats.BaseDamage);
        int damage = healthBeforeAttack - target.Health;
        bool targetDied = !target.IsAlive;

        result = new AttackResult(attacker.Id, target.Id, damage, targetDied);

        if (target is Enemy enemy && targetDied)
            RemoveEnemy(enemy.Id);

        return true;
    }

    /// <summary>Applies one semantic player action to the current game state.</summary>
    public (CreatureActionOutcome Outcome, CreatureDeath? Death) ApplyPlayerAction(
        Direction direction)
        => ApplyMoveAction(Player, direction);

    /// <summary>Applies one previously planned enemy action to the current world.</summary>
    internal (CreatureActionOutcome Outcome, CreatureDeath? Death) ApplyEnemyAction(
        CreatureId enemyId,
        EnemyAction action)
    {
        ArgumentNullException.ThrowIfNull(action);

        if (!_enemies.TryGetValue(enemyId, out Enemy? enemy) || !enemy.IsAlive)
            throw new InvalidOperationException($"Enemy {enemyId} cannot perform an action.");

        return action switch
        {
            EnemyAction.Move move => ApplyMoveAction(enemy, move.Direction),
            EnemyAction.Wait => (
                new CreatureActionOutcome(
                    enemy.Id,
                    CreatureActionResult.Waited,
                    enemy.Position,
                    enemy.Position),
                null),
            _ => throw new ArgumentOutOfRangeException(nameof(action), action, "Unknown enemy action."),
        };
    }

    /// <summary>Removes an enemy from the game and frees its occupied position.</summary>
    /// <returns>
    /// <see langword="true"/> if the enemy was removed; otherwise <see langword="false"/>.
    /// </returns>
    public bool RemoveEnemy(CreatureId enemyId)
    {
        if (!_enemies.TryGetValue(enemyId, out Enemy? enemy))
            return false;

        _occupancy.Remove(enemy.Position, enemy.Id);
        _enemies.Remove(enemy.Id);
        _creatures.Remove(enemy.Id);
        return true;
    }

    /// <summary>Applies a set of already resolved creature movements simultaneously.</summary>
    internal void ApplyResolvedMoves(
        IReadOnlyCollection<(CreatureId CreatureId, Position From, Position To)> moves)
    {
        _occupancy.MoveMany(moves);

        foreach ((CreatureId creatureId, _, Position to) in moves)
            _creatures[creatureId].SetPosition(to);
    }

    private bool CanPlace(Position position)
        => Map.Contains(position)
            && !_occupancy.IsOccupied(position);

    private (CreatureActionOutcome Outcome, CreatureDeath? Death) ApplyMoveAction(
        Creature actor,
        Direction direction)
    {
        Position from = actor.Position;
        Position target = from + direction;
        Creature? targetCreature = GetCreatureAt(target);

        if (targetCreature is not null)
        {
            if (TryAttackCreature(actor.Id, direction, out AttackResult attack))
            {
                CreatureDeath? death = attack.TargetDied
                    ? new CreatureDeath(attack.TargetId, target)
                    : null;

                return (
                    new CreatureActionOutcome(
                        actor.Id,
                        CreatureActionResult.Attacked,
                        from,
                        target),
                    death);
            }

            return (
                new CreatureActionOutcome(
                    actor.Id,
                    CreatureActionResult.Blocked,
                    from,
                    target),
                null);
        }

        return TryMoveCreature(actor.Id, direction)
            ? (
                new CreatureActionOutcome(
                    actor.Id,
                    CreatureActionResult.Moved,
                    from,
                    target),
                null)
            : (
                new CreatureActionOutcome(
                    actor.Id,
                    CreatureActionResult.Blocked,
                    from,
                    target),
                null);
    }

    private void EnsureCanPlace(Position position)
    {
        if (!Map.Contains(position))
        {
            throw new ArgumentException(
                "The position is outside the map.",
                nameof(position));
        }

        if (_occupancy.IsOccupied(position))
            throw new InvalidOperationException($"Position {position} is already occupied.");
    }

    private void AddEnemy(Position position, EnemyType enemyType)
    {
        var enemy = new Enemy(NextCreatureId(), position, enemyType);
        _occupancy.Place(enemy.Position, enemy.Id);
        _creatures.Add(enemy.Id, enemy);
        _enemies.Add(enemy.Id, enemy);
    }

    private CreatureId NextCreatureId()
        => new(_nextCreatureId++);
}
