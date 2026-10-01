using Godot;

public partial class MainMenu : Control
{
    private const string LevelSelectionMenuScenePath =
        "res://UI/LevelSelectionMenu/level_selection_menu.tscn";

    private const string LevelBuilderScenePath =
        "res://UI/LevelBuilder/LevelBuilder.tscn";

    public override void _Ready()
    {
        Button playButton = GetNode<Button>("%PlayButton");
        Button levelBuilderButton = GetNode<Button>("%LevelBuilderButton");
        Button exitButton = GetNode<Button>("%ExitButton");

        playButton.Pressed += OpenLevelSelectionMenu;
        levelBuilderButton.Pressed += OpenLevelBuilder;
        exitButton.Pressed += Exit;
    }

    private void OpenLevelSelectionMenu()
        => GetTree().ChangeSceneToFile(LevelSelectionMenuScenePath);

    private void OpenLevelBuilder()
        => GetTree().ChangeSceneToFile(LevelBuilderScenePath);

    private void Exit()
        => GetTree().Quit();
}
