using UnityEngine;
using UnityEditor;
using Unity.XR.CoreUtils;
using UnityEditor.SceneManagement;

namespace VRDungeonCrawler.Editor
{
    public static class FixXRCameraManually
    {
        [MenuItem("Tools/VR Dungeon Crawler/Force Fix XR Camera (Unpack Prefab)")]
        public static void ForceFixCamera()
        {
            Debug.Log("========================================");
            Debug.Log("FORCE FIXING XR Camera Reference");
            Debug.Log("========================================");

            XROrigin xrOrigin = Object.FindObjectOfType<XROrigin>();
            if (xrOrigin == null)
            {
                Debug.LogError("❌ XROrigin not found!");
                return;
            }

            Camera mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Debug.LogError("❌ Main Camera not found!");
                return;
            }

            Debug.Log($"Found XROrigin: {xrOrigin.gameObject.name}");
            Debug.Log($"Found Main Camera: {mainCamera.gameObject.name}");

            // Check if it's a prefab instance
            bool isPrefab = PrefabUtility.IsPartOfPrefabInstance(xrOrigin.gameObject);
            Debug.Log($"Is Prefab Instance: {isPrefab}");

            if (isPrefab)
            {
                Debug.Log("Unpacking prefab instance...");
                PrefabUtility.UnpackPrefabInstance(xrOrigin.gameObject, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
                Debug.Log("✓ Prefab unpacked!");
            }

            // Now set the camera
            xrOrigin.Camera = mainCamera;
            xrOrigin.RequestedTrackingOriginMode = XROrigin.TrackingOriginMode.Floor;

            EditorUtility.SetDirty(xrOrigin);
            EditorUtility.SetDirty(xrOrigin.gameObject);

            Debug.Log($"✓ XROrigin.Camera = {xrOrigin.Camera?.name ?? "null"}");
            Debug.Log($"✓ Tracking Mode = {xrOrigin.RequestedTrackingOriginMode}");

            // Mark scene dirty and save
            EditorSceneManager.MarkSceneDirty(xrOrigin.gameObject.scene);
            EditorSceneManager.SaveScene(xrOrigin.gameObject.scene);

            Debug.Log("✓✓✓ Scene saved with camera reference!");
            Debug.Log("========================================");
        }
    }
}
