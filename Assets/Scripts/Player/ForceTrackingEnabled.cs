using UnityEngine;
using UnityEngine.InputSystem.XR;

namespace VRDungeonCrawler.Player
{
    /// <summary>
    /// Emergency fix: Forces TrackedPoseDriver to stay enabled every frame
    /// Attach to Main Camera
    /// </summary>
    public class ForceTrackingEnabled : MonoBehaviour
    {
        private TrackedPoseDriver trackedPoseDriver;

        void Start()
        {
            trackedPoseDriver = GetComponent<TrackedPoseDriver>();
            if (trackedPoseDriver == null)
            {
                Debug.LogError("[ForceTrackingEnabled] No TrackedPoseDriver found on Main Camera!");
            }
            else
            {
                Debug.Log("[ForceTrackingEnabled] Found TrackedPoseDriver, will keep it enabled");
            }
        }

        void Update()
        {
            if (trackedPoseDriver != null && !trackedPoseDriver.enabled)
            {
                Debug.LogWarning("[ForceTrackingEnabled] TrackedPoseDriver was disabled! Re-enabling...");
                trackedPoseDriver.enabled = true;
            }
        }
    }
}
