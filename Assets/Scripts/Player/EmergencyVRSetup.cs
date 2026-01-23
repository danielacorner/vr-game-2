using UnityEngine;
using UnityEngine.XR;
using UnityEngine.InputSystem.XR;
using System.Collections.Generic;

namespace VRDungeonCrawler.Player
{
    /// <summary>
    /// Emergency VR setup - forcibly configures head tracking
    /// Attach to XR Origin
    /// </summary>
    public class EmergencyVRSetup : MonoBehaviour
    {
        void Start()
        {
            Debug.Log("========================================");
            Debug.Log("[EmergencyVRSetup] FORCING VR SETUP");
            Debug.Log("========================================");

            // Find Main Camera
            Camera mainCam = Camera.main;
            if (mainCam == null)
            {
                Debug.LogError("[EmergencyVRSetup] NO MAIN CAMERA FOUND!");
                mainCam = GetComponentInChildren<Camera>();
                if (mainCam != null)
                {
                    Debug.Log($"[EmergencyVRSetup] Found camera: {mainCam.name}");
                    mainCam.tag = "MainCamera";
                }
            }

            if (mainCam == null)
            {
                Debug.LogError("[EmergencyVRSetup] CANNOT FIND ANY CAMERA!");
                return;
            }

            // Check XR status
            Debug.Log($"[EmergencyVRSetup] XR enabled: {XRSettings.enabled}");
            Debug.Log($"[EmergencyVRSetup] XR device: {XRSettings.loadedDeviceName}");
            Debug.Log($"[EmergencyVRSetup] XR active: {XRSettings.isDeviceActive}");

            // List all XR devices
            List<XRInputSubsystem> subsystems = new List<XRInputSubsystem>();
            SubsystemManager.GetSubsystems(subsystems);
            Debug.Log($"[EmergencyVRSetup] Found {subsystems.Count} XR input subsystems");

            // Check/add TrackedPoseDriver
            TrackedPoseDriver tpd = mainCam.GetComponent<TrackedPoseDriver>();
            if (tpd == null)
            {
                Debug.LogWarning("[EmergencyVRSetup] NO TrackedPoseDriver found! Adding one...");
                tpd = mainCam.gameObject.AddComponent<TrackedPoseDriver>();
            }

            // Force configure it
            tpd.enabled = true;
            tpd.trackingType = TrackedPoseDriver.TrackingType.RotationAndPosition;
            tpd.updateType = TrackedPoseDriver.UpdateType.UpdateAndBeforeRender;

            Debug.Log($"[EmergencyVRSetup] TrackedPoseDriver configured:");
            Debug.Log($"  - Enabled: {tpd.enabled}");
            Debug.Log($"  - Type: {tpd.trackingType}");
            Debug.Log($"  - Update: {tpd.updateType}");

            // Create visual marker at camera position
            CreateMarker();

            // Start continuous monitoring
            InvokeRepeating(nameof(MonitorTracking), 1f, 2f);
        }

        void CreateMarker()
        {
            Camera mainCam = Camera.main;
            if (mainCam == null) return;

            // Create HUGE BLUE SPHERE at camera position
            GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            marker.name = "CAMERA_POSITION_MARKER";
            marker.transform.position = mainCam.transform.position + mainCam.transform.forward * 2f;
            marker.transform.localScale = Vector3.one * 0.5f;

            Renderer r = marker.GetComponent<Renderer>();
            if (r != null && r.material != null)
            {
                r.material.color = Color.blue;
            }

            Destroy(marker.GetComponent<Collider>());

            Debug.Log($"[EmergencyVRSetup] Created BLUE marker at: {marker.transform.position}");
        }

        void MonitorTracking()
        {
            Camera mainCam = Camera.main;
            if (mainCam == null) return;

            TrackedPoseDriver tpd = mainCam.GetComponent<TrackedPoseDriver>();

            Debug.Log("========== VR TRACKING STATUS ==========");
            Debug.Log($"Camera position: {mainCam.transform.position}");
            Debug.Log($"Camera rotation: {mainCam.transform.eulerAngles}");

            if (tpd != null)
            {
                Debug.Log($"TrackedPoseDriver: {(tpd.enabled ? "ENABLED" : "DISABLED")}");
            }
            else
            {
                Debug.LogError("TrackedPoseDriver: MISSING!");
            }

            Debug.Log("=======================================");
        }
    }
}
