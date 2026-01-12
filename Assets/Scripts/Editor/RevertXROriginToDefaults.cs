using UnityEngine;
using UnityEditor;
using Unity.XR.CoreUtils;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Movement;
using UnityEditor.SceneManagement;

namespace VRDungeonCrawler.Editor
{
    public static class RevertXROriginToDefaults
    {
        [MenuItem("Tools/VR Dungeon Crawler/★ REVERT XR ORIGIN TO DEFAULTS ★", priority = -10)]
        public static void RevertToDefaults()
        {
            XROrigin xrOrigin = Object.FindFirstObjectByType<XROrigin>();
            if (xrOrigin == null)
            {
                EditorUtility.DisplayDialog("Error", "XR Origin not found!", "OK");
                return;
            }

            // Check if it's a prefab instance
            if (!PrefabUtility.IsPartOfPrefabInstance(xrOrigin.gameObject))
            {
                EditorUtility.DisplayDialog("Error", "XR Origin is not a prefab instance!", "OK");
                return;
            }

            Debug.LogError("========================================");
            Debug.LogError("[RevertXROrigin] REVERTING ALL PREFAB OVERRIDES!");
            Debug.LogError("========================================");

            // Revert ALL prefab overrides - this will restore the working input configuration
            PrefabUtility.RevertPrefabInstance(xrOrigin.gameObject, InteractionMode.AutomatedAction);

            Debug.LogError("[RevertXROrigin] ✓ ALL overrides reverted!");

            // NOW set the camera reference (this is required)
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                xrOrigin.Camera = mainCamera;
                EditorUtility.SetDirty(xrOrigin);
                Debug.LogError("[RevertXROrigin] ✓ Camera reference set");
            }

            // Set tracking mode to Floor
            xrOrigin.RequestedTrackingOriginMode = XROrigin.TrackingOriginMode.Floor;
            EditorUtility.SetDirty(xrOrigin);
            Debug.LogError("[RevertXROrigin] ✓ Tracking mode set to Floor");

            // Set move speed to 20 (4x)
            var moveProvider = xrOrigin.GetComponent<ContinuousMoveProvider>();
            if (moveProvider != null)
            {
                moveProvider.moveSpeed = 20f;
                EditorUtility.SetDirty(moveProvider);
                Debug.LogError("[RevertXROrigin] ✓ Move speed set to 20");
            }

            // Save scene
            EditorSceneManager.MarkSceneDirty(xrOrigin.gameObject.scene);
            EditorSceneManager.SaveScene(xrOrigin.gameObject.scene);

            Debug.LogError("========================================");
            Debug.LogError("[RevertXROrigin] ✓✓✓ XR ORIGIN RESTORED!");
            Debug.LogError("[RevertXROrigin] Input actions now use prefab defaults");
            Debug.LogError("[RevertXROrigin] Scene saved - BUILD NOW!");
            Debug.LogError("========================================");

            EditorUtility.DisplayDialog(
                "XR Origin Restored!",
                "XR Origin reverted to working prefab defaults!\n\n" +
                "✓ All input actions restored\n" +
                "✓ Camera reference set\n" +
                "✓ Floor tracking mode\n" +
                "✓ Move speed 20 (4x)\n\n" +
                "BUILD NOW and test!",
                "OK"
            );
        }
    }
}
