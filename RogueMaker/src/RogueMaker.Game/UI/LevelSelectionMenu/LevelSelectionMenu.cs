using Godot;
using System;
using System.Collections.Generic;

public partial class LevelSelectionMenu : Control
{
    private const string MainMenuScenePath =
        "res://UI/MainMenu/main_menu.tscn";

    private GameSession _gameSession = null!;
    private Label _loadStatus = null!;

    public override void _Ready()
    {
        Button backButton = GetNode<Button>("%BackToMainMenuButton");
        VBoxContainer levelList = GetNode<VBoxContainer>("%LevelList");
        _gameSession = GetNode<GameSession>("/root/GameSession");
        _loadStatus = GetNode<Label>("%LoadStatus");

        backButton.Pressed += ReturnToMainMenu;
        PopulateLevelList(levelList);
    }

    private void PopulateLevelList(VBoxContainer levelList)
    {
        string[] levelFiles = GetLevelFiles();

        if (levelFiles.Length == 0)
        {
            levelList.AddChild(new Label
            {
                Text = "No levels",
                HorizontalAlignment = HorizontalAlignment.Center,
            });
            return;
        }

        foreach (string fileName in levelFiles)
        {   // take only name of file without .json
            string levelName = fileName[..^LevelStorage.LevelFileExtension.Length];
            Button levelButton = new() { Text = levelName };

            levelButton.Pressed += () => SelectLevel(fileName);
            levelList.AddChild(levelButton);
        }
    }

    private static string[] GetLevelFiles()
    { // 
        using DirAccess directory = DirAccess.Open(LevelStorage.LevelsDirectoryPath);

        if (directory is null)
            return [];

        List<string> levelFiles = [];

        directory.ListDirBegin();

        for (string fileName = directory.GetNext();
             fileName.Length > 0;
             fileName = directory.GetNext())
        {
            if (!directory.CurrentIsDir()
                && fileName.EndsWith(
                    LevelStorage.LevelFileExtension,
                    StringComparison.OrdinalIgnoreCase))
            {
                levelFiles.Add(fileName);
            }
        }

        directory.ListDirEnd();
        levelFiles.Sort(StringComparer.OrdinalIgnoreCase);

        return [.. levelFiles];
    }

    private void SelectLevel(string fileName)
    {
        try
        {
            _loadStatus.Text = string.Empty;
            _gameSession.Start(LevelStorage.Load(fileName));
        }
        catch (Exception exception)
        {
            _loadStatus.Text = $"Could not load level: {exception.Message}";
            GD.PushError(exception.ToString());
        }
    }

    private void ReturnToMainMenu()
        => GetTree().ChangeSceneToFile(MainMenuScenePath);
}
