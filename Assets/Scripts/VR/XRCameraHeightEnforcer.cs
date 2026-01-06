using UnityEngine;
using Unity.XR.CoreUtils;

namespace VRDungeonCrawler.VR
{
    /// <summary>
    /// Monitors camera height and only corrects if tracking fails completely
    /// Works as a last-resort fallback, not active enforcement
    /// </summary>
    public class XRCameraHeightEnforcer : MonoBehaviour
    {
        [Header("Fallback Height Settings")]
        [Tooltip("Minimum camera height to trigger fallback (meters)")]
        public float fallbackThreshold = 0.5f;

        [Tooltip("Fallback standing height when tracking fails (meters)")]
        public float fallbackHeight = 1.6f;

        [Tooltip("Wait this long before checking (let tracking initialize)")]
        public float initialWaitTime = 2f;

        [Tooltip("Monitor for this many seconds after initial wait")]
        public float monitorDuration = 3f;

        [Header("Debug")]
        public bool logStatus = true;

        private XROrigin xrOrigin;
        private Transform cameraOffsetTransform;
        private float startTime;
        private bool hasChecked = false;
        private bool trackingWorking = false;

        private void Awake()
        {
            xrOrigin = GetComponent<XROrigin>();
            if (xrOrigin != null && xrOrigin.CameraFloorOffsetObject != null)
            {
                cameraOffsetTransform = xrOrigin.CameraFloorOffsetObject.transform;
            }
            startTime = Time.time;
        }

        private void Update()
        {
            float elapsed = Time.time - startTime;

            // Wait for initial tracking initialization
            if (elapsed < initialWaitTime)
            {
                return;
            }

            // Monitor during check period
            if (elapsed < initialWaitTime + monitorDuration && !hasChecked)
            {
                CheckAndEnforceFallback();
            }
            else if (!hasChecked)
            {
                hasChecked = true;
                if (xrOrigin != null && xrOrigin.Camera != null)
                {
                    float finalHeight = xrOrigin.CameraInOriginSpacePos.y;
                    if (logStatus)
                    {
                        Debug.Log($"[CameraHeightEnforcer] Monitoring complete. Final camera height: {finalHeight:F2}m. Tracking is {(trackingWorking ? "WORKING" : "INACTIVE")}");
                    }
                }
            }
        }

        private void CheckAndEnforceFallback()
        {
            if (xrOrigin == null || xrOrigin.Camera == null) return;

            // Check if tracking is providing position updates
            Vector3 cameraLocalPos = xrOrigin.Camera.transform.localPosition;
            float currentCameraHeight = xrOrigin.CameraInOriginSpacePos.y;

            // If camera has local position data, tracking is working
            if (cameraLocalPos.magnitude > 0.01f)
            {
                trackingWorking = true;
                return; // Tracking is working, don't interfere!
            }

            // If we get here, tracking might be stuck
            // Only apply fallback if camera is at floor level (< 0.5m)
            if (currentCameraHeight < fallbackThreshold)
            {
                if (cameraOffsetTransform != null && !trackingWorking)
                {
                    // Apply fallback height ONLY if tracking is not working
                    Vector3 localPos = cameraOffsetTransform.localPosition;
                    if (Mathf.Abs(localPos.y - fallbackHeight) > 0.01f)
                    {
                        localPos.y = fallbackHeight;
                        cameraOffsetTransform.localPosition = localPos;

                        if (logStatus)
                        {
                            Debug.LogWarning($"[CameraHeightEnforcer] Applied fallback height ({fallbackHeight:F2}m) - tracking appears inactive");
                        }
                    }
                }
            }
        }

        // Allow manual enforcement via context menu
        [ContextMenu("Apply Fallback Height Now")]
        public void ApplyFallbackHeight()
        {
            if (cameraOffsetTransform != null)
            {
                Vector3 localPos = cameraOffsetTransform.localPosition;
                localPos.y = fallbackHeight;
                cameraOffsetTransform.localPosition = localPos;
                Debug.Log($"[CameraHeightEnforcer] Manually applied fallback height: {fallbackHeight:F2}m");
            }
        }
    }
}
