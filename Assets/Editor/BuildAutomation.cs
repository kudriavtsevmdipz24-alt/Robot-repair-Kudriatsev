using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class BuildAutomation
{
    // Запуск (Windows):
    // Unity.exe -batchmode -quit -projectPath C:\Users\MAX\Desktop\labs\gamedis -executeMethod BuildAutomation.BuildWindowsRelease
    public static void BuildWindowsRelease()
    {
        const string exeName = "Robot Repair - Flood Run.exe";
        const string version = "1.0.3";

        string projectRoot = Directory.GetParent(Application.dataPath)!.FullName;
        string labsRoot = Directory.GetParent(projectRoot)!.FullName;
        string outputDir = Path.Combine(labsRoot, "Builds", $"RobotRepair-FloodRun-v{version}-Windows");

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