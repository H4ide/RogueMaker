using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using Godot;
using RogueMaker.Core.Enemies;
using RogueMaker.Core.Levels;
using RogueMaker.Core.Map;

/// <summary>Serializes level drafts and stores them in the user levels directory.</summary>
internal static class LevelStorage
{
    public const string LevelsDirectoryPath = "user://levels";
    public const string LevelFileExtension = ".json";

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() },
    };

    /// <summary>
    /// Saves a complete level draft as JSON under the current local date and time.
    /// </summary>
    /// <param name="level">The draft to save.</param>
    /// <returns>The resulting file name, including the JSON extension.</returns>
    public static string Save(LevelDraft level)
    {
        string fileName = DateTime.Now.ToString(
            "yyyy-MM-dd_HH-mm-ss-ff")
            + LevelFileExtension;
        string directoryPath = ProjectSettings.GlobalizePath(LevelsDirectoryPath);
        Directory.CreateDirectory(directoryPath);

        string json = JsonSerializer.Serialize(
            SavedLevel.From(level),
            SerializerOptions);

        File.WriteAllText(Path.Combine(directoryPath, fileName), json);
        return fileName;
    }

    /// <summary>Loads a JSON level file and restores its editable draft.</summary>
    /// <param name="fileName">The level file name inside <see cref="LevelsDirectoryPath"/>.</param>
    /// <returns>The level draft represented by the file.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="fileName"/> is not a plain JSON file name.
    /// </exception>
    /// <exception cref="InvalidDataException">
    /// Thrown when the file does not contain a valid saved level.
    /// </exception>
    public static LevelDraft Load(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName)
            || !fileName.EndsWith(LevelFileExtension, StringComparison.OrdinalIgnoreCase)
            || fileName != Path.GetFileName(fileName))
        {
            throw new ArgumentException(
                "A level file name must be a JSON file without a directory path.",
                nameof(fileName));
        }

        string directoryPath = ProjectSettings.GlobalizePath(LevelsDirectoryPath);
        string filePath = Path.Combine(directoryPath, fileName);
        string json = File.ReadAllText(filePath);

        try
        {
            SavedLevel savedLevel = JsonSerializer.Deserialize<SavedLevel>(
                json,
                SerializerOptions)
                ?? throw new InvalidDataException("The level file is empty.");

            return savedLevel.ToDraft();
        }
        catch (JsonException exception)
        {
            throw new InvalidDataException(
                $"'{fileName}' does not contain valid level JSON.",
                exception);
        }
    }
}

/// <summary>The complete serializable representation of one level draft.</summary>
internal sealed record SavedLevel(
    int Width,
    int Height,
    SurfaceType[][] Surfaces,
    Position PlayerPosition,
    SavedEnemy[] Enemies)
{
    /// <summary>Copies mutable draft data into a value intended for serialization.</summary>
    public static SavedLevel From(LevelDraft level)
    {
        SurfaceType[][] surfaces = new SurfaceType[level.Height][];

        for (int y = 0; y < level.Height; y++)
        {
            surfaces[y] = new SurfaceType[level.Width];

            for (int x = 0; x < level.Width; x++)
                surfaces[y][x] = level.GetSurface(new Position(x, y));
        }

        List<SavedEnemy> enemies = [];

        foreach ((Position position, EnemyTypeId enemyType) in level.Enemies)
            enemies.Add(new SavedEnemy(position, enemyType));

        return new SavedLevel(
            level.Width,
            level.Height,
            surfaces,
            level.PlayerPosition,
            [.. enemies]);
    }

    /// <summary>Restores an editable draft from the saved values.</summary>
    /// <exception cref="InvalidDataException">
    /// Thrown when the saved dimensions, surfaces, player, or enemies are invalid.
    /// </exception>
    public LevelDraft ToDraft()
    {
        ValidateDimensionsAndSurfaces();

        var level = new LevelDraft(Width, Height);

        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
                level.TrySetSurface(new Position(x, y), Surfaces[y][x]);
        }

        if (!level.Contains(PlayerPosition))
            throw new InvalidDataException("The player position is outside the level.");

        if (PlayerPosition != level.PlayerPosition)
            level.TryMovePlayer(PlayerPosition);

        if (Enemies is null)
            throw new InvalidDataException("The enemy collection is missing.");

        HashSet<Position> occupiedEnemyPositions = [];

        foreach (SavedEnemy enemy in Enemies)
        {
            if (enemy is null)
                throw new InvalidDataException("The enemy collection contains a null entry.");

            if (!Enum.IsDefined(enemy.EnemyType))
                throw new InvalidDataException($"Unknown enemy type: {enemy.EnemyType}.");

            if (!level.Contains(enemy.Position))
                throw new InvalidDataException($"Enemy position {enemy.Position} is outside the level.");

            if (enemy.Position == PlayerPosition)
                throw new InvalidDataException("The player and an enemy occupy the same position.");

            if (!occupiedEnemyPositions.Add(enemy.Position))
                throw new InvalidDataException($"Multiple enemies occupy position {enemy.Position}.");

            level.TrySetEnemy(enemy.Position, enemy.EnemyType);
        }

        return level;
    }

    private void ValidateDimensionsAndSurfaces()
    {
        if (Width is <= 0 or > GameMap.MaxWidth)
            throw new InvalidDataException($"Level width must be between 1 and {GameMap.MaxWidth}.");

        if (Height is <= 0 or > GameMap.MaxHeight)
            throw new InvalidDataException($"Level height must be between 1 and {GameMap.MaxHeight}.");

        if (Surfaces is null || Surfaces.Length != Height)
            throw new InvalidDataException("The surface row count does not match the level height.");

        for (int y = 0; y < Height; y++)
        {
            SurfaceType[] row = Surfaces[y];

            if (row is null || row.Length != Width)
                throw new InvalidDataException($"Surface row {y} does not match the level width.");

            for (int x = 0; x < Width; x++)
            {
                if (!Enum.IsDefined(row[x]))
                {
                    throw new InvalidDataException(
                        $"Unknown surface type at position ({x}, {y}): {row[x]}.");
                }
            }
        }
    }
}

/// <summary>An enemy type and its position in a saved level.</summary>
internal sealed record SavedEnemy(Position Position, EnemyTypeId EnemyType);
