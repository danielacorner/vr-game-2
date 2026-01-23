using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace VRDungeonCrawler.Editor
{
    public static class VerifyBootstrapSetup
    {
        [MenuItem("Tools/VR Dungeon Crawler/Verify Bootstrap Setup")]
        public static void Verify()
        {
            Debug.Log("========================================");
            Debug.Log("[VerifyBootstrapSetup] Checking Bootstrap Setup...");
            Debug.Log("========================================");

            // Check Build Settings
            var scenes = EditorBuildSettings.scenes;
            Debug.Log($"[VerifyBootstrapSetup] Build Settings has {scenes.Length} scenes:");
            
            for (int i = 0; i < scenes.Length; i++)
            {
                var scene = scenes[i];
                Debug.Log($"  [{i}] {scene.path} (enabled: {scene.enabled})");
            }

            // Check if Bootstrap is at index 0
            if (scenes.Length > 0 && scenes[0].path.EndsWith("Bootstrap.unity"))
            {
                Debug.Log("[VerifyBootstrapSetup] ✓ Bootstrap scene is at index 0");
            }
            else
            {
                Debug.LogError("[VerifyBootstrapSetup] ✗ Bootstrap scene is NOT at index 0!");
            }

            // Check if current scene is Bootstrap
            var activeScene = SceneManager.GetActiveScene();
            Debug.Log($"[VerifyBootstrapSetup] Active scene: {activeScene.name} at {activeScene.path}");

            // Check if BootstrapManager exists
            var manager = GameObject.Find("BootstrapManager");
            if (manager != null)
            {
                var script = manager.GetComponent<VRDungeonCrawler.Core.BootstrapManager>();
                if (script != null)
                {
                    Debug.Log($"[VerifyBootstrapSetup] ✓ BootstrapManager found");
                }
                else
                {
                    Debug.LogError("[VerifyBootstrapSetup] ✗ BootstrapManager GameObject found but script is missing!");
                }
            }
            else
            {
                Debug.LogError("[VerifyBootstrapSetup] ✗ BootstrapManager GameObject not found in scene!");
            }

            Debug.Log("========================================");
            Debug.Log("[VerifyBootstrapSetup] Verification complete");
            Debug.Log("========================================");
        }
    }
}
