using UnityEngine;
using UnityEngine.InputSystem.XR;
using UnityEngine.XR;

namespace VRDungeonCrawler.Player
{
    /// <summary>
    /// Diagnostic tool to debug VR head tracking issues
    /// Attach to Main Camera
    /// </summary>
    public class VRTrackingDiagnostic : MonoBehaviour
    {
        private TrackedPoseDriver trackedPoseDriver;
        private Vector3 lastPosition;
        private Quaternion lastRotation;
        private float checkInterval = 1f;
        private float nextCheckTime = 0f;

        void Start()
        {
            trackedPoseDriver = GetComponent<TrackedPoseDriver>();
            lastPosition = transform.localPosition;
            lastRotation = transform.localRotation;

            Debug.Log("========================================");
            Debug.Log("[VRTrackingDiagnostic] STARTING DIAGNOSTICS");
            Debug.Log("========================================");

            // Check XR system
            Debug.Log($"[VRTrackingDiagnostic] XR Settings enabled: {XRSettings.enabled}");
            Debug.Log($"[VRTrackingDiagnostic] XR Device name: {XRSettings.loadedDeviceName}");
            Debug.Log($"[VRTrackingDiagnostic] XR Device active: {XRSettings.isDeviceActive}");

            // Check TrackedPoseDriver
            if (trackedPoseDriver != null)
            {
                Debug.Log($"[VRTrackingDiagnostic] TrackedPoseDriver found: ENABLED={trackedPoseDriver.enabled}");
                Debug.Log($"[VRTrackingDiagnostic] Tracking Type: {trackedPoseDriver.trackingType}");
                Debug.Log($"[VRTrackingDiagnostic] Update Type: {trackedPoseDriver.updateType}");
            }
            else
            {
                Debug.LogError("[VRTrackingDiagnostic] ✗✗✗ NO TrackedPoseDriver component found!");
            }

            // Check camera hierarchy
            Debug.Log($"[VRTrackingDiagnostic] Camera world position: {transform.position}");
            Debug.Log($"[VRTrackingDiagnostic] Camera local position: {transform.localPosition}");
            Debug.Log($"[VRTrackingDiagnostic] Camera parent: {(transform.parent != null ? transform.parent.name : "NULL")}");

            InvokeRepeating(nameof(CheckTracking), 1f, checkInterval);
        }

        void CheckTracking()
        {
            Vector3 currentPos = transform.localPosition;
            Quaternion currentRot = transform.localRotation;

            float posChange = Vector3.Distance(currentPos, lastPosition);
            float rotChange = Quaternion.Angle(currentRot, lastRotation);

            if (posChange > 0.001f || rotChange > 0.1f)
            {
                Debug.Log($"[VRTrackingDiagnostic] ✓ TRACKING IS WORKING! Pos change: {posChange:F3}m, Rot change: {rotChange:F1}°");
            }
            else
            {
                Debug.LogWarning($"[VRTrackingDiagnostic] ✗ NO TRACKING DETECTED for {checkInterval}s - Camera not moving!");
            }

            lastPosition = currentPos;
            lastRotation = currentRot;
        }

        void Update()
        {
            // Force tracking every frame
            if (trackedPoseDriver != null && !trackedPoseDriver.enabled)
            {
                Debug.LogError("[VRTrackingDiagnostic] ✗✗✗ TrackedPoseDriver was DISABLED! Re-enabling...");
                trackedPoseDriver.enabled = true;
            }
        }

        void OnDestroy()
        {
            CancelInvoke(nameof(CheckTracking));
        }
    }
}
