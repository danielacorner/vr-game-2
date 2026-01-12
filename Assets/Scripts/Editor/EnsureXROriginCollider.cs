using UnityEngine;
using UnityEditor;

namespace VRDungeonCrawler.Editor
{
    /// <summary>
    /// Ensures XR Origin has a CharacterController for portal collision detection
    /// </summary>
    public static class EnsureXROriginCollider
    {
        [MenuItem("Tools/VR Dungeon Crawler/Ensure XR Origin Collider")]
        public static void AddCollider()
        {
            Debug.Log("========================================");
            Debug.Log("Checking XR Origin Collider");
            Debug.Log("========================================");

            // Find XR Origin
            GameObject xrOrigin = GameObject.Find("XR Origin (XR Rig)");
            if (xrOrigin == null)
            {
                Debug.LogError("❌ XR Origin (XR Rig) not found in scene!");
                return;
            }

            // Check if CharacterController exists
            CharacterController controller = xrOrigin.GetComponent<CharacterController>();

            if (controller == null)
            {
                // Add CharacterController
                controller = xrOrigin.AddComponent<CharacterController>();
                Debug.Log("✓ Added CharacterController to XR Origin");
            }
            else
            {
                Debug.Log("✓ CharacterController already exists on XR Origin");
            }

            // Configure CharacterController
            controller.center = new Vector3(0, 0.9f, 0); // Center at ~chest height
            controller.radius = 0.3f; // Comfortable radius
            controller.height = 1.8f; // Average human height
            controller.skinWidth = 0.01f;
            controller.minMoveDistance = 0.001f;

            Debug.Log($"✓ CharacterController configured:");
            Debug.Log($"  Center: {controller.center}");
            Debug.Log($"  Radius: {controller.radius}");
            Debug.Log($"  Height: {controller.height}");

            // Mark as dirty
            EditorUtility.SetDirty(xrOrigin);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene()
            );

            Debug.Log("========================================");
            Debug.Log("✓ XR Origin collider setup complete!");
            Debug.Log("========================================");
        }
    }
}
