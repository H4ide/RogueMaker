using System;
using Godot;
using RogueMaker.Core.Levels;
using RogueMaker.Core.Turns;

/// <summary>Creates a game from a selected level and transfers its turn manager.</summary>
public partial class GameSession : Node
{
    private const string GameScenePath = "res://Game/Game.tscn";

    private TurnManager _pendingTurnManager = null!;

    /// <summary>Creates a game from a level draft and opens the gameplay scene.</summary>
    public void Start(LevelDraft level)
    {
        ArgumentNullException.ThrowIfNull(level);

        if (_pendingTurnManager is not null)
            throw new InvalidOperationException("Another game is already waiting for the gameplay scene.");

        _pendingTurnManager = TurnManager.Create(level);
        Error result = GetTree().ChangeSceneToFile(GameScenePath);

        if (result == Error.Ok)
            return;

        _pendingTurnManager = null!;
        throw new InvalidOperationException(
            $"Could not open the gameplay scene: {result}.");
    }

    /// <summary>Returns the pending turn manager exactly once.</summary>
    public TurnManager TakeTurnManager()
    {
        if (_pendingTurnManager is null)
            throw new InvalidOperationException("No game is waiting for the gameplay scene.");

        TurnManager turnManager = _pendingTurnManager;
        _pendingTurnManager = null!;
        return turnManager;
    }
}
