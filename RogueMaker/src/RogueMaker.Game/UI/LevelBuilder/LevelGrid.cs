using System;
using Godot;
using RogueMaker.Core.Enemies;
using RogueMaker.Core.Levels;
using RogueMaker.Core.Map;

/// <summary>
/// LevelGrid Draws using <see cref="LevelDraft"/>
/// Converts mouse coordinates to cell coordinates and reports the selected cell.
/// Mouse click
/// ->
/// LevelGrid._GuiInput()
/// ->
/// EditAt()
/// ->
/// CellEdited(position)
/// ->
/// LevelBuilder.EditCell()
/// ->
/// The selected tool modifies LevelDraft
/// ->
/// LevelGrid.QueueRedraw()
/// ->
/// Godot calls LevelGrid._Draw()
/// </summary>
/// <remarks>
/// This control owns grid rendering and cell hit testing only. Camera navigation
/// is handled by <see cref="LevelEditorView"/>.
/// </remarks>
public partial class LevelGrid : Control
{
    private const float CellSize = 80.0f;

    private LevelDraft _level = null!;
    private GameTextures _textures = null!;
    private Position? _lastEditedPosition;

    /// <summary>
    /// Raised when the user selects a new grid position with the left mouse button.
    /// </summary>
    public event Action<Position> CellEdited;

    /// <summary>The complete rendered size of the current level.</summary>
    public Vector2 ContentSize => _level is null
        ? Vector2.Zero
        : new Vector2(_level.Width, _level.Height) * CellSize;

    /// <summary>Displays a level draft using the supplied texture collection.
    /// This method is called from <see cref="LevelBuilder.CreateLevel()"/></summary>
    /// <param name="level">The mutable draft to display.</param>
    /// <param name="textures">Textures for surfaces, enemies, and the player.</param>
    public void Display(LevelDraft level, GameTextures textures)
    {
        _level = level;
        _textures = textures;
        _lastEditedPosition = null;
        ResizeToContent();
    }

    /// <inheritdoc />
    public override void _GuiInput(InputEvent @event)
    {
        switch (@event)
        {
            case InputEventMouseButton button
                when button.ButtonIndex == MouseButton.Left:
                _lastEditedPosition = null;

                if (button.Pressed)
                    EditAt(button.Position);

                AcceptEvent();
                break;

            case InputEventMouseMotion motion:
                if ((motion.ButtonMask & MouseButtonMask.Left) != 0)
                {
                    EditAt(motion.Position);
                    AcceptEvent();
                }
                else
                {
                    _lastEditedPosition = null;
                }

                break;
        }
    }

    /// <inheritdoc />
    public override void _Draw()
    {
        if (_level is null)
            return;

        for (int y = 0; y < _level.Height; y++)
        {
            for (int x = 0; x < _level.Width; x++)
            {
                Position position = new(x, y);
                SurfaceType surface = _level.GetSurface(position);

                DrawTextureRect(
                    _textures.Surfaces[surface],
                    GetCellRect(position),
                    tile: false);
            }
        }

        DrawEnemies();
        DrawTextureRect(
            _textures.Player,
            GetCellRect(_level.PlayerPosition),
            tile: false);
    }

    /// <summary>Updates the control size and schedules its contents to be redrawn.</summary>
    private void ResizeToContent()
    {
        Size = ContentSize;
        QueueRedraw();
    }

    /// <summary>Draws every enemy stored in the current draft.</summary>
    private void DrawEnemies()
    {
        foreach ((Position position, EnemyTypeId enemyType) in _level.Enemies)
        {
            DrawTextureRect(
                _textures.Enemies[enemyType],
                GetCellRect(position),
                tile: false);
        }
    }

    /// <summary>Raises one edit for the cell under a grid-local pointer position.</summary>
    /// <param name="localPosition">The pointer position in grid-local pixels.</param>
    private void EditAt(Vector2 localPosition)
    {
        Position? position = GetPositionAt(localPosition);

        if (position is null || position == _lastEditedPosition)
            return;

        _lastEditedPosition = position;
        CellEdited?.Invoke(position.Value);
    }

    /// <summary>Finds the level position under a grid-local pointer position.</summary>
    /// <param name="localPosition">The pointer position in grid-local pixels.</param>
    /// <returns>The level position, or <see langword="null"/> when outside the grid.</returns>
    private Position? GetPositionAt(Vector2 localPosition)
    {
        if (_level is null)
            return null;

        Position position = new(
            Mathf.FloorToInt(localPosition.X / CellSize),
            Mathf.FloorToInt(localPosition.Y / CellSize));

        return _level.Contains(position)
            ? position
            : null;
    }

    /// <summary>Returns the grid-local rectangle occupied by a level position.</summary>
    /// <param name="position">The level position to convert.</param>
    /// <returns>The rendered cell rectangle in grid-local pixels.</returns>
    private Rect2 GetCellRect(Position position)
        => new(
            position.X * CellSize,
            position.Y * CellSize,
            CellSize,
            CellSize);
}
