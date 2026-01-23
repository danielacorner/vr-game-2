using UnityEngine;
using UnityEngine.InputSystem.XR;
using UnityEngine.XR;

namespace VRDungeonCrawler.Player
{
    /// <summary>
    /// Aggressively forces head tracking to stay enabled
    /// Runs every frame to ensure TrackedPoseDriver is working
    /// </summary>
    public class ForceHeadTracking : MonoBehaviour
    {
        private TrackedPoseDriver trackedPoseDriver;
        private Camera mainCamera;
        private int frameCount = 0;

        void Start()
        {
            Debug.Log("========================================");
            Debug.Log("[ForceHeadTracking] STARTING AGGRESSIVE HEAD TRACKING MONITOR");
            Debug.Log("========================================");

            FindAndConfigureTracking();
        }

        void Update()
        {
            frameCount++;

            // Check every 30 frames (about 0.5 seconds at 60fps)
            if (frameCount % 30 == 0)
            {
                CheckAndForceTracking();
            }
        }

        void FindAndConfigureTracking()
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Debug.LogError("[ForceHeadTracking] NO MAIN CAMERA FOUND!");
                return;
            }

            trackedPoseDriver = mainCamera.GetComponent<TrackedPoseDriver>();
            if (trackedPoseDriver == null)
            {
                Debug.LogWarning("[ForceHeadTracking] NO TrackedPoseDriver on camera, adding one...");
                trackedPoseDriver = mainCamera.gameObject.AddComponent<TrackedPoseDriver>();
            }

            ForceEnableTracking();
        }

        void ForceEnableTracking()
        {
            if (trackedPoseDriver == null) return;

            trackedPoseDriver.enabled = true;
            trackedPoseDriver.trackingType = TrackedPoseDriver.TrackingType.RotationAndPosition;
            trackedPoseDriver.updateType = TrackedPoseDriver.UpdateType.UpdateAndBeforeRender;

            // Try to enable Input Actions if they exist
            if (trackedPoseDriver.positionAction != null)
            {
                try
                {
                    trackedPoseDriver.positionAction.Enable();
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"[ForceHeadTracking] Could not enable positionAction: {e.Message}");
                }
            }

            if (trackedPoseDriver.rotationAction != null)
            {
                try
                {
                    trackedPoseDriver.rotationAction.Enable();
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"[ForceHeadTracking] Could not enable rotationAction: {e.Message}");
                }
            }

            Debug.Log("[ForceHeadTracking] ✓ Forced tracking enabled");
        }

        void CheckAndForceTracking()
        {
            if (mainCamera == null)
            {
                mainCamera = Camera.main;
                if (mainCamera == null) return;
            }

            if (trackedPoseDriver == null)
            {
                trackedPoseDriver = mainCamera.GetComponent<TrackedPoseDriver>();
                if (trackedPoseDriver == null)
                {
                    Debug.LogWarning($"[ForceHeadTracking] Frame {frameCount}: TrackedPoseDriver MISSING, re-adding...");
                    trackedPoseDriver = mainCamera.gameObject.AddComponent<TrackedPoseDriver>();
                    ForceEnableTracking();
                    return;
                }
            }

            // Check if it got disabled
            if (!trackedPoseDriver.enabled)
            {
                Debug.LogWarning($"[ForceHeadTracking] Frame {frameCount}: TrackedPoseDriver was DISABLED, re-enabling...");
                ForceEnableTracking();
            }

            // Log XR status periodically
            if (frameCount % 120 == 0) // Every 2 seconds
            {
                Debug.Log("========== HEAD TRACKING STATUS ==========");
                Debug.Log($"XR Enabled: {XRSettings.enabled}");
                Debug.Log($"XR Device: {XRSettings.loadedDeviceName}");
                Debug.Log($"XR Active: {XRSettings.isDeviceActive}");
                Debug.Log($"Camera Position: {mainCamera.transform.position}");
                Debug.Log($"Camera Rotation: {mainCamera.transform.eulerAngles}");
                Debug.Log($"TrackedPoseDriver Enabled: {trackedPoseDriver.enabled}");
                Debug.Log($"TrackedPoseDriver Type: {trackedPoseDriver.trackingType}");
                Debug.Log("=========================================");
            }
        }
    }
}
