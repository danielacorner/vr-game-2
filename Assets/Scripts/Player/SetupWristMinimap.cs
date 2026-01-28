using UnityEngine;

namespace VRDungeonCrawler.Player
{
    /// <summary>
    /// Editor helper to setup WristMinimap component
    /// Run this once to add WristMinimap to the scene
    /// </summary>
    public class SetupWristMinimap : MonoBehaviour
    {
        [ContextMenu("Setup WristMinimap Component")]
        void SetupMinimap()
        {
            // Check if WristMinimap already exists
            WristMinimap existing = FindFirstObjectByType<WristMinimap>();

            if (existing != null)
            {
                Debug.LogWarning($"[SetupWristMinimap] WristMinimap already exists on: {existing.gameObject.name}");
                Debug.LogWarning($"  Current offsets: Left={existing.leftWristOffset}, Right={existing.rightWristOffset}");

                // Fix the offsets
                existing.leftWristOffset = new Vector3(0f, 0f, -0.15f);
                existing.rightWristOffset = new Vector3(0f, 0f, -0.15f);
                existing.alwaysVisible = true; // For testing
                existing.aggressiveLogging = true; // For debugging

                Debug.LogWarning($"  FIXED offsets: Left={existing.leftWristOffset}, Right={existing.rightWristOffset}");
                Debug.LogWarning("  Enabled alwaysVisible and aggressiveLogging for testing");

                #if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(existing);
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(existing.gameObject.scene);
                #endif
            }
            else
            {
                Debug.LogError("[SetupWristMinimap] No WristMinimap found in scene!");
                Debug.LogError("  Creating new WristMinimap component on this GameObject...");

                WristMinimap minimap = gameObject.AddComponent<WristMinimap>();
                minimap.leftWristOffset = new Vector3(0f, 0f, -0.15f);
                minimap.rightWristOffset = new Vector3(0f, 0f, -0.15f);
                minimap.alwaysVisible = true;
                minimap.aggressiveLogging = true;

                Debug.Log("[SetupWristMinimap] Created WristMinimap component with correct offsets");

                #if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(minimap);
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(gameObject.scene);
                #endif
            }
        }

        [ContextMenu("Find All WristMinimap Instances")]
        void FindAllInstances()
        {
            WristMinimap[] all = FindObjectsByType<WristMinimap>(FindObjectsSortMode.None);

            Debug.Log($"[SetupWristMinimap] Found {all.Length} WristMinimap instance(s):");

            foreach (WristMinimap minimap in all)
            {
                Debug.Log($"  GameObject: {minimap.gameObject.name}");
                Debug.Log($"    Scene: {minimap.gameObject.scene.name}");
                Debug.Log($"    Path: {GetGameObjectPath(minimap.gameObject)}");
                Debug.Log($"    Left Offset: {minimap.leftWristOffset}");
                Debug.Log($"    Right Offset: {minimap.rightWristOffset}");
                Debug.Log($"    Always Visible: {minimap.alwaysVisible}");
                Debug.Log("  ---");
            }
        }

        string GetGameObjectPath(GameObject obj)
        {
            string path = obj.name;
            Transform parent = obj.transform.parent;

            while (parent != null)
            {
                path = parent.name + "/" + path;
                parent = parent.parent;
            }

            return path;
        }
    }
}
