using UnityEditor;
using UnityEngine;

namespace VRDungeonCrawler.Editor
{
    public static class BuildAndRun
    {
        [MenuItem("Tools/VR Dungeon Crawler/Build and Run on Quest")]
        public static void BuildAndRunOnQuest()
        {
            Debug.Log("========================================");
            Debug.Log("Building and Running on Quest 3...");
            Debug.Log("========================================");

            // Get current build settings
            string buildPath = EditorUserBuildSettings.GetBuildLocation(BuildTarget.Android);
            if (string.IsNullOrEmpty(buildPath) || !System.IO.Directory.Exists(System.IO.Path.GetDirectoryName(buildPath)))
            {
                buildPath = "builds/vr-game-2.apk";
            }

            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
            buildPlayerOptions.scenes = new[] { "Assets/Scenes/HomeArea.unity" };
            buildPlayerOptions.locationPathName = buildPath;
            buildPlayerOptions.target = BuildTarget.Android;
            buildPlayerOptions.options = BuildOptions.AutoRunPlayer;

            Debug.Log($"Build path: {buildPath}");

            var report = BuildPipeline.BuildPlayer(buildPlayerOptions);

            if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
            {
                Debug.Log("✓✓✓ Build succeeded!");
                Debug.Log($"Build size: {report.summary.totalSize} bytes");
                Debug.Log("Deploying to Quest 3...");
            }
            else
            {
                Debug.LogError($"❌ Build failed: {report.summary.result}");
            }
        }
    }
}
