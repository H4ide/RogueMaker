using RogueMaker.Core.Actions;
using RogueMaker.Core.Creatures;
using RogueMaker.Core.Game;

namespace RogueMaker.Core.Turns;

/// <summary>Starts one planning task per enemy using the same immutable world snapshot.</summary>
public static class EnemyTurnPlanner
{
    public static Task<PlannedEnemyAction>[] Plan(WorldSnapshot world)
    {
        var planTasks = new Task<PlannedEnemyAction>[world.Enemies.Count];

        for (int index = 0; index < world.Enemies.Count; index++)
        {
            EnemySnapshot enemy = world.Enemies[index];
            planTasks[index] = Task.Run(() =>
            {
                EnemyAction action = enemy.Brain.Decide(world, enemy);
                return new PlannedEnemyAction(enemy.Id, action);
            });
        }

        return planTasks;
    }
}
