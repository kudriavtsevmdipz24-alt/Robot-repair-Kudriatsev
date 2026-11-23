using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class BuildAutomation
{
    public static void BuildWindowsRelease()
    {
        string projectRoot = Directory.GetParent(Application.dataPath)!.FullName;
        string version = PlayerSettings.bundleVersion;
        string[] args = Environment.GetCommandLineArgs();
        int outputArg = Array.IndexOf(args, "-buildOutput");
        string outputPath = outputArg >= 0 && outputArg + 1 < args.Length
            ? Path.GetFullPath(args[outputArg + 1], projectRoot)
            : Path.Combine(projectRoot, "Builds", $"robot-repair-timed-gate-v{version}-windows", $"{PlayerSettings.productName}.exe");
        Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);

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
