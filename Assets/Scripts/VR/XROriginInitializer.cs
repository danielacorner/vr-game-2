using UnityEngine;
using UnityEngine.XR;
using Unity.XR.CoreUtils;
using System.Collections;
using System.Collections.Generic;

namespace VRDungeonCrawler.VR
{
    /// <summary>
    /// Ensures XR Origin is properly initialized with Floor tracking mode on startup
    /// Fixes the issue where viewpoint starts at floor level until Oculus menu is pressed
    /// </summary>
    [RequireComponent(typeof(XROrigin))]
    public class XROriginInitializer : MonoBehaviour
    {
        [Header("Tracking Settings")]
        [Tooltip("Use Floor level tracking (recommended for Quest)")]
        public bool useFloorTracking = true;

        [Tooltip("Initial delay before starting XR initialization (seconds)")]
        public float initialDelay = 0.5f;

        [Tooltip("Maximum attempts to initialize tracking")]
        public int maxAttempts = 5;

        [Tooltip("Delay between initialization attempts (seconds)")]
        public float attemptDelay = 0.3f;

        private XROrigin xrOrigin;

        private void Awake()
        {
            xrOrigin = GetComponent<XROrigin>();
        }

        private void Start()
        {
            // Start the initialization coroutine
            StartCoroutine(InitializeTrackingMode());
        }

        private IEnumerator InitializeTrackingMode()
        {
            if (xrOrigin == null)
            {
                Debug.LogError("[XROriginInitializer] XROrigin component not found!");
                yield break;
            }

            Debug.Log("[XROriginInitializer] Waiting for XR tracking to become active...");

            // CRITICAL: Wait for XR tracking to actually start working
            bool trackingActive = false;
            float maxWaitTime = 5f;
            float waitStartTime = Time.time;

            while (!trackingActive && (Time.time - waitStartTime) < maxWaitTime)
            {
                // Check if XR subsystem exists and is running
                List<XRInputSubsystem> subsystems = new List<XRInputSubsystem>();
                SubsystemManager.GetSubsystems(subsystems);

                if (subsystems.Count > 0 && subsystems[0].running)
                {
                    // Check if we're actually getting tracking data
                    if (xrOrigin.Camera != null)
                    {
                        Vector3 cameraPos = xrOrigin.Camera.transform.localPosition;
                        // If camera is not at exactly (0,0,0), tracking is working
                        if (cameraPos.magnitude > 0.01f)
                        {
                            trackingActive = true;
                            Debug.Log($"[XROriginInitializer] ✓ XR tracking is ACTIVE! Camera at local: {cameraPos}");
                        }
                    }
                }

                if (!trackingActive)
                {
                    yield return new WaitForSeconds(0.1f);
                }
            }

            if (!trackingActive)
            {
                Debug.LogWarning("[XROriginInitializer] XR tracking did not activate after 5 seconds. Proceeding anyway...");
            }

            // Additional delay to ensure tracking is stable
            yield return new WaitForSeconds(initialDelay);

            Debug.Log("[XROriginInitializer] Configuring tracking mode...");

            // Now configure tracking mode
            List<XRInputSubsystem> finalSubsystems = new List<XRInputSubsystem>();
            SubsystemManager.GetSubsystems(finalSubsystems);

            if (finalSubsystems.Count > 0)
            {
                XRInputSubsystem inputSubsystem = finalSubsystems[0];

                // Set tracking origin mode
                TrackingOriginModeFlags requestedMode = useFloorTracking ?
                    TrackingOriginModeFlags.Floor :
                    TrackingOriginModeFlags.Device;

                if (inputSubsystem.TrySetTrackingOriginMode(requestedMode))
                {
                    Debug.Log($"[XROriginInitializer] ✓ Tracking mode set to {requestedMode}");
                }

                // Get current tracking origin mode to verify
                TrackingOriginModeFlags currentMode = inputSubsystem.GetTrackingOriginMode();
                Debug.Log($"[XROriginInitializer] Current tracking mode: {currentMode}");
            }

            // Set on XROrigin component as well
            xrOrigin.RequestedTrackingOriginMode = useFloorTracking ?
                XROrigin.TrackingOriginMode.Floor :
                XROrigin.TrackingOriginMode.Device;

            yield return new WaitForSeconds(0.5f);

            // Log final camera position
            if (xrOrigin.Camera != null)
            {
                float cameraY = xrOrigin.CameraInOriginSpacePos.y;
                Debug.Log($"[XROriginInitializer] ✓✓✓ Initialization complete. Camera height: {cameraY:F2}m");
            }
        }

        // Optional: Add method to manually recenter if needed
        [ContextMenu("Recenter Tracking")]
        public void RecenterTracking()
        {
            if (xrOrigin != null)
            {
                List<XRInputSubsystem> subsystems = new List<XRInputSubsystem>();
                SubsystemManager.GetSubsystems(subsystems);

                if (subsystems.Count > 0)
                {
                    subsystems[0].TryRecenter();
                    Debug.Log("[XROriginInitializer] Manual recenter requested");
                }

                xrOrigin.MoveCameraToWorldLocation(xrOrigin.transform.position);
            }
        }
    }
}
