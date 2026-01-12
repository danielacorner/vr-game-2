using UnityEngine;

namespace VRDungeonCrawler.VR
{
    /// <summary>
    /// Minimal diagnostic script to test if scripts execute on XR Origin
    /// </summary>
    [DefaultExecutionOrder(-200)] // Run even earlier
    public class XRDiagnostic : MonoBehaviour
    {
        private void Awake()
        {
            Debug.Log("========================================");
            Debug.Log("========================================");
            Debug.Log("========================================");
            Debug.Log("[XRDiagnostic] AWAKE CALLED!");
            Debug.Log($"[XRDiagnostic] GameObject: {gameObject.name}");
            Debug.Log($"[XRDiagnostic] Active: {gameObject.activeSelf}");
            Debug.Log($"[XRDiagnostic] Component enabled: {enabled}");
            Debug.Log("========================================");
            Debug.Log("========================================");
            Debug.Log("========================================");
        }

        private void Start()
        {
            Debug.Log("[XRDiagnostic] START CALLED!");
        }

        private void OnEnable()
        {
            Debug.Log("[XRDiagnostic] ONENABLE CALLED!");
        }
    }
}