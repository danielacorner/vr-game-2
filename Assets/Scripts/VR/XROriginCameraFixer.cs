using UnityEngine;
using Unity.XR.CoreUtils;

namespace VRDungeonCrawler.VR
{
    /// <summary>
    /// Ensures XR Origin has proper camera reference on startup
    /// This fixes head tracking issues
    /// </summary>
    [RequireComponent(typeof(XROrigin))]
    public class XROriginCameraFixer : MonoBehaviour
    {
        private void Awake()
        {
            XROrigin xrOrigin = GetComponent<XROrigin>();

            if (xrOrigin.Camera == null)
            {
                // Find Main Camera
                Camera mainCamera = Camera.main;

                if (mainCamera != null)
                {
                    xrOrigin.Camera = mainCamera;
                    Debug.Log($"[XROriginCameraFixer] ✓ Set XROrigin.Camera to {mainCamera.name}");
                }
                else
                {
                    Debug.LogError("[XROriginCameraFixer] ❌ Could not find Main Camera!");
                }
            }
            else
            {
                Debug.Log($"[XROriginCameraFixer] XROrigin.Camera already set to {xrOrigin.Camera.name}");
            }
        }
    }
}
