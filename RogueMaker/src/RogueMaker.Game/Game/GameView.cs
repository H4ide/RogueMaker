using System;
using Godot;
using RogueMaker.Core.Creatures;
using RogueMaker.Core.Game;
using RogueMaker.Core.Map;

/// <summary>
/// Renders immutable world snapshots without owning the live game state.
/// </summary>
public partial class GameView : Node2D
{
    private const float CellSize = 80.0f;

    /// <summary>Textures used to draw surfaces and creatures.</summary>
    [Export]
    public GameTextures Textures { get; set; } = null!;

    private Camera2D _camera = null!;
    private Label _gameInfo = null!;
    private WorldSnapshot _world = null!;

    /// <inheritdoc />
    public override void _Ready()
    {
        _camera = GetNode<Camera2D>("%PlayerCamera");
        _gameInfo = GetNode<Label>("%GameInfo");
        _camera.MakeCurrent();
    }

    /// <summary>Displays the world produced before or after a complete turn.</summary>
    public void Display(
        WorldSnapshot world,
        int turnNumber,
        GameStatus status)
    {
        bool isFirstDisplay = _world is null;
        _world = world;
        _camera.Position = GetCellRect(world.Player.Position).GetCenter();

        if (isFirstDisplay)
            _camera.ResetSmoothing();

        QueueRedraw();

        _gameInfo.Text =
            $"Turn: {turnNumber}\n" +
            $"Status: {status}\n" +
            $"Map: {world.Map.Width} x {world.Map.Height}\n" +
            $"Player: ({world.Player.Position.X}, {world.Player.Position.Y})\n" +
            $"Health: {world.Player.Health}/{world.Player.Stats.MaxHealth}\n" +
            $"Enemies: {world.Enemies.Count}\n\n" +
            "Move: WASD or arrow keys";
    }

    /// <inheritdoc />
    public override void _Draw()
    {
        if (_world is null)
            return;

        for (int y = 0; y < _world.Map.Height; y++)
        {
            for (int x = 0; x < _world.Map.Width; x++)
            {
                Position position = new(x, y);
                SurfaceType surface = _world.Map.GetSurface(position).Type;

                DrawTextureRect(
                    Textures.Surfaces[surface],
                    GetCellRect(position),
                    tile: false);
            }
        }

        foreach (EnemySnapshot enemy in _world.Enemies)
        {
            DrawTextureRect(
                Textures.Enemies[enemy.TypeId],
                GetCellRect(enemy.Position),
                tile: false);
        }

        DrawTextureRect(
            Textures.Player,
            GetCellRect(_world.Player.Position),
            tile: false);
    }

    /// <summary>Displays an error produced while performing a game action.</summary>
    public void DisplayError(string message)
        => _gameInfo.Text = $"Could not perform turn:\n{message}";

    private static Rect2 GetCellRect(Position position)
        => new(
            position.X * CellSize,
            position.Y * CellSize,
            CellSize,
            CellSize);
}
