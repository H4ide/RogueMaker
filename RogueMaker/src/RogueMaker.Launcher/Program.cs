using System.Diagnostics;

internal static class Program
{
    // Might not work on other pc, need to set own GodotExecutable path.
    private const string GodotExecutable = @"C:\Godot\Godot_v4.7-stable_mono_win64.exe";
    // from src\RogueMaker.Launcher\bin\Debug\net8.0 to src\RogueMaker.Game
    private const string GameProjectRelativePath = @"..\..\..\..\RogueMaker.Game";

    private static void Main()
    {
        string gameProjectDirectory = Path.GetFullPath(
            GameProjectRelativePath,
            AppContext.BaseDirectory);

        var startInfo = new ProcessStartInfo(GodotExecutable);
        startInfo.ArgumentList.Add("--path");
        startInfo.ArgumentList.Add(gameProjectDirectory);

        Process.Start(startInfo);
    }
}
