using UnityEngine;
using TMPro;
using UnityEngine.InputSystem.XR;

namespace VRDungeonCrawler.Player
{
    /// <summary>
    /// Creates a 3D text display showing tracking status
    /// Will appear in front of player in VR
    /// </summary>
    public class TrackingStatusDisplay : MonoBehaviour
    {
        private GameObject textObj;
        private TextMeshPro textMesh;
        private TrackedPoseDriver trackedPoseDriver;
        private Vector3 lastPos;
        private Quaternion lastRot;
        private int updateCount = 0;

        void Start()
        {
            // Get TrackedPoseDriver from Main Camera
            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                trackedPoseDriver = mainCam.GetComponent<TrackedPoseDriver>();
            }

            // Create 3D text in front of spawn point
            textObj = new GameObject("TrackingStatus_Text");
            textObj.transform.position = transform.position + transform.forward * 3f + Vector3.up * 2f;
            textObj.transform.rotation = Quaternion.LookRotation(textObj.transform.position - transform.position);

            textMesh = textObj.AddComponent<TextMeshPro>();
            textMesh.fontSize = 1;
            textMesh.alignment = TextAlignmentOptions.Center;
            textMesh.color = Color.white;
            textMesh.text = "Initializing...";

            // Make it face the camera
            textObj.transform.LookAt(transform.position);
            textObj.transform.Rotate(0, 180, 0);

            lastPos = transform.position;
            lastRot = transform.rotation;
        }

        void Update()
        {
            if (textMesh == null) return;

            updateCount++;

            // Check if camera moved
            Vector3 currentPos = transform.position;
            Quaternion currentRot = transform.rotation;

            float posChange = Vector3.Distance(currentPos, lastPos);
            float rotChange = Quaternion.Angle(currentRot, lastRot);

            string status = "=== VR TRACKING STATUS ===\n\n";

            if (trackedPoseDriver != null)
            {
                status += $"TrackedPoseDriver: {(trackedPoseDriver.enabled ? "ENABLED" : "DISABLED")}\n";
                status += $"Type: {trackedPoseDriver.trackingType}\n\n";
            }
            else
            {
                status += "TrackedPoseDriver: NOT FOUND!\n\n";
            }

            status += $"Camera Pos: {currentPos:F2}\n";
            status += $"Pos Change: {posChange:F3}m\n";
            status += $"Rot Change: {rotChange:F1}°\n\n";

            if (posChange > 0.01f || rotChange > 1f)
            {
                status += "<color=green>✓ TRACKING WORKING!</color>\n";
            }
            else
            {
                status += "<color=red>✗ NO TRACKING DETECTED</color>\n";
            }

            status += $"\nUpdates: {updateCount}";

            textMesh.text = status;

            lastPos = currentPos;
            lastRot = currentRot;

            // Keep text facing camera
            if (Camera.main != null)
            {
                Vector3 directionToCamera = Camera.main.transform.position - textObj.transform.position;
                if (directionToCamera != Vector3.zero)
                {
                    textObj.transform.rotation = Quaternion.LookRotation(-directionToCamera);
                }
            }
        }
    }
}
