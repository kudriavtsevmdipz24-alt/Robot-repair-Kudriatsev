using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;

public static class BuildAutomation
{
    // Запуск:
    // Unity.exe -batchmode -quit -projectPath D:\Robot-repair-Kudriatsev -executeMethod BuildAutomation.BuildWindowsRelease
    public static void BuildWindowsRelease()
    {
        const string outputDir = @"D:\Builds\RobotRepair-FloodRun-v1.0-Windows";
        const string exeName = "Robot Repair - Flood Run.exe";

        Directory.CreateDirectory(outputDir);
        string outputPath = Path.Combine(outputDir, exeName);

        string[] scenes = EditorBuildSettings.scenes
            .Where(s => s.enabled)
            .Select(s => s.path)
            .ToArray();

        if (scenes.Length == 0)
        {
            throw new Exception("No enabled scenes in Build Settings.");
        }

        var options = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = outputPath,
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.None
        };

        BuildReport report = BuildPipeline.BuildPlayer(options);
        var summary = report.summary;
        if (summary.result != BuildResult.Succeeded)
        {
            throw new Exception($"Build failed: {summary.result}");
        }

        UnityEngine.Debug.Log($"BUILD OK: {outputPath}");
    }
}
