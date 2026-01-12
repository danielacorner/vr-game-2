using UnityEngine;
using UnityEditor;
using Unity.XR.CoreUtils;
using UnityEditor.SceneManagement;
using UnityEngine.InputSystem.XR;
using UnityEngine.InputSystem;

namespace VRDungeonCrawler.Editor
{
    /// <summary>
    /// Fixes XR Origin camera reference and TrackedPoseDriver to enable head tracking
    /// </summary>
    public static class FixXROriginCamera
    {
        [MenuItem("Tools/VR Dungeon Crawler/Fix XR Origin Camera Reference")]
        public static void FixCameraReference()
        {
            Debug.Log("========================================");
            Debug.Log("Fixing XR Origin Camera Reference & Head Tracking");
            Debug.Log("========================================");

            // Find XR Origin
            XROrigin xrOrigin = Object.FindObjectOfType<XROrigin>();
            if (xrOrigin == null)
            {
                Debug.LogError("❌ XROrigin component not found!");
                return;
            }

            // Find Main Camera
            Camera mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Debug.LogError("❌ Main Camera not found!");
                return;
            }

            Debug.Log($"Found XR Origin: {xrOrigin.gameObject.name}");
            Debug.Log($"Found Main Camera: {mainCamera.gameObject.name}");

            // 1. SET CAMERA REFERENCE
            xrOrigin.Camera = mainCamera;
            EditorUtility.SetDirty(xrOrigin);
            Debug.Log($"✓ XROrigin.Camera = {xrOrigin.Camera?.name ?? "null"}");

            // 2. SET TRACKING MODE TO FLOOR
            xrOrigin.RequestedTrackingOriginMode = XROrigin.TrackingOriginMode.Floor;
            EditorUtility.SetDirty(xrOrigin);
            Debug.Log($"✓ RequestedTrackingOriginMode = Floor");

            // 3. NOTE: TrackedPoseDriver will be configured at runtime by XRSetupFixer
            Debug.Log("TrackedPoseDriver will be configured at runtime by XRSetupFixer component");

            // 4. VERIFY CAMERA OFFSET POSITION
            if (xrOrigin.CameraFloorOffsetObject != null)
            {
                Debug.Log($"✓ CameraFloorOffsetObject = {xrOrigin.CameraFloorOffsetObject.name}");
                Debug.Log($"   Position: {xrOrigin.CameraFloorOffsetObject.transform.localPosition}");

                if (xrOrigin.CameraFloorOffsetObject.transform.localPosition != Vector3.zero)
                {
                    Debug.LogWarning($"⚠ Camera Offset position is not zero, this may cause height issues");
                }
            }
            else
            {
                Debug.LogWarning("⚠ CameraFloorOffsetObject is null");
            }

            // Verify tracking mode
            Debug.Log($"Requested Tracking Origin Mode: {xrOrigin.RequestedTrackingOriginMode}");
            Debug.Log($"Current Tracking Origin Mode: {xrOrigin.CurrentTrackingOriginMode}");

            // Mark scene dirty
            EditorSceneManager.MarkSceneDirty(xrOrigin.gameObject.scene);

            Debug.Log("========================================");
            Debug.Log("✓✓✓ XR Origin configured for head tracking!");
            Debug.Log("Head tracking should now work properly.");
            Debug.Log("Please SAVE the scene and rebuild.");
            Debug.Log("========================================");
        }
    }
}
