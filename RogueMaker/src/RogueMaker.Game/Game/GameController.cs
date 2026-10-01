using System;
using System.Threading.Tasks;
using Godot;
using RogueMaker.Core.Game;
using RogueMaker.Core.Map;
using RogueMaker.Core.Turns;

/// <summary>
/// Translates player input into game actions and sends immutable results to the UI.
/// </summary>
public partial class GameController : Node
{
    private const string MainMenuScenePath =
        "res://UI/MainMenu/main_menu.tscn";

    private TurnManager _turnManager = null!;
    private GameView _view = null!;
    private bool _turnInProgress;

    /// <inheritdoc />
    public override void _Ready()
    {
        GameSession session = GetNode<GameSession>("/root/GameSession");
        Button backButton = GetNode<Button>("%BackButton");
        _view = GetNode<GameView>("%GameView");

        _turnManager = session.TakeTurnManager();
        _view.Display(
            _turnManager.CreateWorldSnapshot(),
            _turnManager.TurnNumber,
            _turnManager.Status);

        backButton.Pressed += ReturnToMainMenu;
    }

    /// <inheritdoc />
    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is not InputEventKey key
            || !key.Pressed
            || key.Echo)
        {
            return;
        }

        Direction? direction = key.Keycode switch
        {
            Key.W or Key.Up => Direction.Up,
            Key.S or Key.Down => Direction.Down,
            Key.A or Key.Left => Direction.Left,
            Key.D or Key.Right => Direction.Right,
            _ => null,
        };

        if (direction is not Direction playerDirection)
            return;

        GetViewport().SetInputAsHandled();

        if (_turnInProgress || _turnManager.Status != GameStatus.InProgress)
            return;

        _ = PerformTurnAsync(playerDirection);
    }

    private async Task PerformTurnAsync(Direction direction)
    {
        _turnInProgress = true;

        try
        {
            TurnResult result =
                await _turnManager.PerformGameActionAsync(direction);

            _view.Display(
                result.World,
                result.TurnNumber,
                result.Status);
        }
        catch (Exception exception)
        {
            _view.DisplayError(exception.Message);
            GD.PushError(exception.ToString());
        }
        finally
        {
            _turnInProgress = false;
        }
    }

    private void ReturnToMainMenu()
        => GetTree().ChangeSceneToFile(MainMenuScenePath);
}
