using RogueMaker.Core.Map;

namespace RogueMaker.Core.Actions;

/// <summary>An action proposed by enemy AI without changing the game world.</summary>
public abstract record EnemyAction
{
    private EnemyAction()
    {
    }

    /// <summary>Attempts to move one cell in the specified direction.</summary>
    public sealed record Move(Direction Direction) : EnemyAction;

    /// <summary>stays in the current position.</summary>
    public sealed record Wait : EnemyAction;
}
