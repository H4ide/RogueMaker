using System;
using System.Collections.Generic;
using Godot;
using RogueMaker.Core.Enemies;
using RogueMaker.Core.Levels;
using RogueMaker.Core.Map;

/// <summary>
/// Creates level drafts and connects the editing tool palette to the level grid.
/// <remarks>
/// interesting idea with the delegate: we store a function that edits a cell in _applySelectedTool and 
/// invoke it later when the user clicks a grid position. This allows us to have a single event handler 
/// for all tools, and we can easily add new tools by adding new functions to the list of tool actions.
/// So we don't need to write a separate event handler for each tool, 
/// which would be awful for future updates if the plan to add more enemies types/player types/surfaces.
/// </remarks>
/// </summary>
public partial class LevelBuilder : Control
{
    private const int MinimumLevelSize = 1;

    private const string MainMenuScenePath =
        "res://UI/MainMenu/main_menu.tscn";

    /// <summary>Textures used by the tool palette and level-grid preview.</summary>
    [Export]
    public GameTextures Textures { get; set; } = null!;

    private LevelDraft _currentLevel = null!;
    private CenterContainer _sizeSetup = null!;
    private HBoxContainer _editorWorkspace = null!;
    private LevelEditorView _editorView = null!;
    private LevelGrid _levelGrid = null!;
    private ItemList _toolList = null!;
    private readonly List<Func<Position, bool>> _toolActions = [];
    private Func<Position, bool> _applySelectedTool = null!;

    /// <inheritdoc />
    public override void _Ready()
    {
        Button backButton = GetNode<Button>("%BackButton");
        Button createButton = GetNode<Button>("%CreateButton");
        SpinBox widthSpinBox = GetNode<SpinBox>("%WidthSpinBox");
        SpinBox heightSpinBox = GetNode<SpinBox>("%HeightSpinBox");
        Button saveButton = GetNode<Button>("%SaveButton");
        Label saveStatus = GetNode<Label>("%SaveStatus");
        _sizeSetup = GetNode<CenterContainer>("%SizeSetup");
        _editorWorkspace = GetNode<HBoxContainer>("%EditorWorkspace");
        _editorView = GetNode<LevelEditorView>("%EditorView");
        _levelGrid = GetNode<LevelGrid>("%LevelGrid");
        _toolList = GetNode<ItemList>("%ToolList");

        backButton.Pressed += ReturnToMainMenu;
        createButton.Pressed += () => CreateLevel(widthSpinBox, heightSpinBox);
        saveButton.Pressed += () => SaveLevel(saveStatus);
        // we select item (wall,enemy,player, etc) and because of delegate we store the function that edits a cell in _applySelectedTool
        _toolList.ItemSelected += SelectTool;
        // if anything changed (return true if changed) we redraw the grid
        _levelGrid.CellEdited += EditCell;
        widthSpinBox.MinValue = MinimumLevelSize;
        widthSpinBox.MaxValue = GameMap.MaxWidth;
        heightSpinBox.MinValue = MinimumLevelSize;
        heightSpinBox.MaxValue = GameMap.MaxHeight;
        widthSpinBox.GetLineEdit().FocusMode = FocusModeEnum.None;
        heightSpinBox.GetLineEdit().FocusMode = FocusModeEnum.None;

        AddTools();
    }

    /// <summary>Registers all surface, player, and enemy editing tools.</summary>
    private void AddTools()
    {
        foreach (SurfaceType surface in Enum.GetValues<SurfaceType>())
        {
            AddTool(
                surface.ToString(),
                position => _currentLevel.TrySetSurface(position, surface),
                Textures.Surfaces[surface]);

        }

        AddTool(
            "Player",
            position => _currentLevel.TryMovePlayer(position),
            Textures.Player);

        foreach (EnemyTypeId enemyType in Enum.GetValues<EnemyTypeId>())
        {
            AddTool(
                enemyType.ToString(),
                position => _currentLevel.TrySetEnemy(position, enemyType),
                Textures.Enemies[enemyType]);
        }

        AddTool(
            "Remove enemy",
            position => _currentLevel.TryRemoveEnemy(position));
    }

    /// <summary>
    /// Adds a palette item and stores the function that edits a selected cell.
    /// </summary>
    /// <remarks>
    /// The item and its <paramref name="action"/> are appended in the same order in _toolActions as in _toolList, so
    /// the returned index identifies both of them.
    /// it is invoked later by <see cref="EditCell"/> after the user selects this tool
    /// and clicks a grid position. Returning <see langword="true"/> from the function
    /// means that the level changed and we should redraw the map.
    /// </remarks>
    /// <param name="name">The label displayed in the tool palette.</param>
    /// <param name="action">
    /// A function that accepts the selected <see cref="Position"/> and returns whether
    /// applying the tool changed the level.
    /// </param>
    /// <param name="texture">An optional icon displayed beside the tool name.</param>
    /// <returns>The palette index assigned to the tool.</returns>
    /// <example>
    /// The following tool changes a selected cell to a wall:
    /// <code>
    /// AddTool(
    ///     "Wall",
    ///     position => _currentLevel.TrySetSurface(
    ///         position,
    ///         SurfaceType.Wall),
    ///     Textures.Surfaces[SurfaceType.Wall]);
    /// </code>
    /// The lambda is stored as a <see cref="Func{T, TResult}"/> and called only when
    /// the user applies the selected tool to a cell.
    /// </example>
    private void AddTool(
        string name,
        Func<Position, bool> action,
        Texture2D texture = null!)
    {
        _toolList.AddItem(name, texture);
        _toolActions.Add(action);
    }

    /// <summary>Selects the editing action associated with a palette item.</summary>
    /// <remarks>
    /// The selected tool's action is stored in <see cref="_applySelectedTool"/> and invoked when the user clicks a grid position.
    /// for example , if the user selects the "Wall" tool, the action will be a function that sets the surface of the clicked cell to a wall.
    /// </remarks>
    /// <param name="index">The selected palette index.</param>
    private void SelectTool(long index)
        => _applySelectedTool = _toolActions[(int)index];

    /// <summary>Creates a draft with the requested dimensions and opens the editor.</summary>
    /// <param name="widthSpinBox">The control containing the requested width.</param>
    /// <param name="heightSpinBox">The control containing the requested height.</param>
    private void CreateLevel(SpinBox widthSpinBox, SpinBox heightSpinBox)
    {
        int width = (int)widthSpinBox.Value;
        int height = (int)heightSpinBox.Value;
        _currentLevel = new LevelDraft(width, height);

        _sizeSetup.Hide();
        _editorWorkspace.Show();
        _levelGrid.Display(_currentLevel, Textures);
        _editorView.ResetView();
    }

    /// <summary>Applies the selected tool to one grid position.</summary>
    /// <param name="position">The position selected on the grid.</param>
    private void EditCell(Position position)
    {
        if (_applySelectedTool is not null && _applySelectedTool(position))
            _levelGrid.QueueRedraw();
    }

    /// <summary>Saves the current draft under a name based on the current time.</summary>
    private void SaveLevel(Label saveStatus)
    {
        try
        {
            string fileName = LevelStorage.Save(_currentLevel);
            saveStatus.Text = $"Saved: {fileName}";
        }
        catch (Exception exception)
        {
            saveStatus.Text = $"Could not save: {exception.Message}";
            GD.PushError(exception.ToString());
        }
    }

    /// <summary>Leaves the level builder and opens the main menu.</summary>
    private void ReturnToMainMenu()
        => GetTree().ChangeSceneToFile(MainMenuScenePath);
}
