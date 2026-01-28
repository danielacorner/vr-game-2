using UnityEngine;

namespace VRDungeonCrawler.Player
{
    /// <summary>
    /// Add this component to any GameObject in your scene to create the WristMinimap
    /// Run once in HomeArea scene, then it persists via DontDestroyOnLoad
    /// </summary>
    public class WristMinimapSetup : MonoBehaviour
    {
        [Header("Setup")]
        [Tooltip("Check this to create the minimap on Awake")]
        public bool createOnStart = true;

        [Tooltip("Enable for debugging (shows minimap always, logs every frame)")]
        public bool debugMode = false;

        void Awake()
        {
            if (createOnStart)
            {
                SetupMinimap();
            }
        }

        [ContextMenu("Setup WristMinimap Now")]
        void SetupMinimap()
        {
            // Check if one already exists
            WristMinimap existing = FindFirstObjectByType<WristMinimap>();

            if (existing != null)
            {
                Debug.LogWarning($"[WristMinimapSetup] WristMinimap already exists on: {existing.gameObject.name}");
                Debug.LogWarning($"  Current offsets: Left={existing.leftWristOffset}, Right={existing.rightWristOffset}");

                // Fix the offsets (negative Y to move down toward wrist - 30cm)
                existing.leftWristOffset = new Vector3(0f, -0.3f, 0f);
                existing.rightWristOffset = new Vector3(0f, -0.3f, 0f);
                existing.alwaysVisible = debugMode;
                existing.aggressiveLogging = debugMode;

                Debug.LogWarning($"  UPDATED to: Left={existing.leftWristOffset}, Right={existing.rightWristOffset}");

                #if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(existing);
                #endif
                return;
            }

            // Create new GameObject for the minimap
            GameObject minimapObj = new GameObject("WristMinimap_System");
            WristMinimap minimap = minimapObj.AddComponent<WristMinimap>();

            // Set correct offset values (negative Y = down toward wrist - 30cm)
            minimap.leftWristOffset = new Vector3(0f, -0.3f, 0f);
            minimap.rightWristOffset = new Vector3(0f, -0.3f, 0f);
            minimap.alwaysVisible = debugMode;
            minimap.aggressiveLogging = debugMode;

            Debug.Log("[WristMinimapSetup] ✓ Created WristMinimap component!");
            Debug.LogWarning($"  Left offset: {minimap.leftWristOffset}");
            Debug.LogWarning($"  Right offset: {minimap.rightWristOffset}");
            Debug.Log($"  Debug mode: {debugMode}");

            #if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(minimapObj);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(minimapObj.scene);
            #endif
        }

        [ContextMenu("Find Existing WristMinimap")]
        void FindExisting()
        {
            WristMinimap[] all = FindObjectsByType<WristMinimap>(FindObjectsSortMode.None);

            if (all.Length == 0)
            {
                Debug.LogWarning("[WristMinimapSetup] No WristMinimap found in scene!");
                Debug.Log("  Run 'Setup WristMinimap Now' from context menu to create one.");
            }
            else
            {
                Debug.Log($"[WristMinimapSetup] Found {all.Length} WristMinimap instance(s):");

                foreach (WristMinimap minimap in all)
                {
                    Debug.Log($"  GameObject: {minimap.gameObject.name}");
                    Debug.Log($"    Scene: {minimap.gameObject.scene.name}");
                    Debug.Log($"    Left Offset: {minimap.leftWristOffset}");
                    Debug.Log($"    Right Offset: {minimap.rightWristOffset}");
                    Debug.Log($"    Always Visible: {minimap.alwaysVisible}");
                    Debug.Log("  ---");
                }
            }
        }
    }
}
