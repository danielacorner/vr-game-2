using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
using System.Collections;

namespace VRDungeonCrawler.VR
{
    /// <summary>
    /// CRITICAL FIX: Forces TrackedPoseDriver input actions to be enabled
    /// This fixes the bug where head tracking doesn't work because Position and Rotation actions are disabled
    /// </summary>
    public class ForceEnableHeadTracking : MonoBehaviour
    {
        [Header("Input Asset")]
        [Tooltip("XRI Default Input Actions asset")]
        public InputActionAsset inputActionAsset;

        [Header("Debug")]
        public bool showDebugLogs = true;

        private TrackedPoseDriver trackedPoseDriver;
        private bool hasEnabledActions = false;

        private void Awake()
        {
            Debug.LogError("========================================");
            Debug.LogError("[ForceEnableHeadTracking] AWAKE CALLED!");
            Debug.LogError("========================================");

            // Find the Main Camera's TrackedPoseDriver
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                trackedPoseDriver = mainCamera.GetComponent<TrackedPoseDriver>();
                if (trackedPoseDriver == null)
                {
                    Debug.LogError("[ForceEnableHeadTracking] NO TrackedPoseDriver found on Main Camera!");
                }
                else
                {
                    Debug.LogError($"[ForceEnableHeadTracking] Found TrackedPoseDriver on {mainCamera.name}");
                }
            }
            else
            {
                Debug.LogError("[ForceEnableHeadTracking] NO Main Camera found!");
            }
        }

        private void Start()
        {
            StartCoroutine(EnableActionsDelayed());
        }

        private IEnumerator EnableActionsDelayed()
        {
            // Wait for XR to initialize
            yield return new WaitForSeconds(0.5f);

            // Try multiple times to ensure success
            for (int attempt = 0; attempt < 5; attempt++)
            {
                Debug.LogError($"========================================");
                Debug.LogError($"[ForceEnableHeadTracking] ATTEMPT {attempt + 1}/5");
                Debug.LogError($"========================================");

                if (EnableHeadTrackingActions())
                {
                    hasEnabledActions = true;
                    Debug.LogError("========================================");
                    Debug.LogError("[ForceEnableHeadTracking] ✓✓✓ SUCCESS!");
                    Debug.LogError("[ForceEnableHeadTracking] Head tracking actions ENABLED!");
                    Debug.LogError("========================================");
                    yield break;
                }

                yield return new WaitForSeconds(0.3f);
            }

            Debug.LogError("========================================");
            Debug.LogError("[ForceEnableHeadTracking] ❌ FAILED after 5 attempts!");
            Debug.LogError("========================================");
        }

        private bool EnableHeadTrackingActions()
        {
            if (inputActionAsset == null)
            {
                Debug.LogError("[ForceEnableHeadTracking] InputActionAsset is NULL!");
                return false;
            }

            // Find the XRI HMD action map
            InputActionMap hmdMap = inputActionAsset.FindActionMap("XRI HMD");
            if (hmdMap == null)
            {
                hmdMap = inputActionAsset.FindActionMap("XRI Head");
            }

            if (hmdMap == null)
            {
                Debug.LogError("[ForceEnableHeadTracking] Could not find XRI HMD or XRI Head action map!");
                return false;
            }

            Debug.LogError($"[ForceEnableHeadTracking] Found action map: {hmdMap.name}");

            // Get the Position and Rotation actions
            InputAction positionAction = hmdMap.FindAction("Position");
            InputAction rotationAction = hmdMap.FindAction("Rotation");

            if (positionAction == null || rotationAction == null)
            {
                Debug.LogError($"[ForceEnableHeadTracking] Position: {positionAction != null}, Rotation: {rotationAction != null}");
                return false;
            }

            // Check current status
            Debug.LogError($"[ForceEnableHeadTracking] BEFORE:");
            Debug.LogError($"  Position enabled: {positionAction.enabled}");
            Debug.LogError($"  Rotation enabled: {rotationAction.enabled}");

            // Enable the entire action map first
            if (!hmdMap.enabled)
            {
                hmdMap.Enable();
                Debug.LogError($"[ForceEnableHeadTracking] Enabled action map: {hmdMap.name}");
            }

            // Enable individual actions
            if (!positionAction.enabled)
            {
                positionAction.Enable();
                Debug.LogError("[ForceEnableHeadTracking] Enabled Position action");
            }

            if (!rotationAction.enabled)
            {
                rotationAction.Enable();
                Debug.LogError("[ForceEnableHeadTracking] Enabled Rotation action");
            }

            // Verify they're enabled
            bool posEnabled = positionAction.enabled;
            bool rotEnabled = rotationAction.enabled;

            Debug.LogError($"[ForceEnableHeadTracking] AFTER:");
            Debug.LogError($"  Position enabled: {posEnabled}");
            Debug.LogError($"  Rotation enabled: {rotEnabled}");

            return posEnabled && rotEnabled;
        }

        private void Update()
        {
            // Continuously check and re-enable if something disabled them
            if (hasEnabledActions && Time.frameCount % 300 == 0) // Check every 5 seconds
            {
                if (inputActionAsset != null)
                {
                    InputActionMap hmdMap = inputActionAsset.FindActionMap("XRI HMD");
                    if (hmdMap == null) hmdMap = inputActionAsset.FindActionMap("XRI Head");

                    if (hmdMap != null)
                    {
                        InputAction positionAction = hmdMap.FindAction("Position");
                        InputAction rotationAction = hmdMap.FindAction("Rotation");

                        if (positionAction != null && !positionAction.enabled)
                        {
                            Debug.LogWarning("[ForceEnableHeadTracking] Position action was disabled! Re-enabling...");
                            positionAction.Enable();
                        }

                        if (rotationAction != null && !rotationAction.enabled)
                        {
                            Debug.LogWarning("[ForceEnableHeadTracking] Rotation action was disabled! Re-enabling...");
                            rotationAction.Enable();
                        }
                    }
                }
            }
        }

        [ContextMenu("Force Enable Now")]
        public void ForceEnableNow()
        {
            EnableHeadTrackingActions();
        }
    }
}
