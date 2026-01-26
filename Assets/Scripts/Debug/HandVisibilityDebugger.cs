using UnityEngine;
using System.Text;

namespace VRDungeonCrawler.Diagnostics
{
    /// <summary>
    /// Diagnostic tool to debug invisible hands issue
    /// Logs detailed information about hand GameObject state, renderers, materials, etc.
    /// Attach this to any GameObject in the scene to run diagnostics
    /// </summary>
    public class HandVisibilityDebugger : MonoBehaviour
    {
        [Header("Debug Settings")]
        [Tooltip("Log diagnostics every N seconds")]
        public float logInterval = 2f;

        [Tooltip("Draw debug visualization")]
        public bool drawGizmos = true;

        private float nextLogTime = 0f;
        private GameObject leftHand;
        private GameObject rightHand;
        private Camera mainCamera;

        void Start()
        {
            Debug.Log("========================================");
            Debug.Log("[HandVisibilityDebugger] Starting hand visibility diagnostics...");
            Debug.Log("========================================");

            FindHands();
            mainCamera = Camera.main;

            if (mainCamera == null)
            {
                Debug.LogError("[HandVisibilityDebugger] Main camera not found!");
            }
        }

        void Update()
        {
            if (Time.time >= nextLogTime)
            {
                nextLogTime = Time.time + logInterval;
                RunDiagnostics();
            }
        }

        void FindHands()
        {
            // Method 1: Find by name
            leftHand = GameObject.Find("PolytopiaHand_L");
            rightHand = GameObject.Find("PolytopiaHand_R");

            if (leftHand != null)
            {
                Debug.Log($"[HandVisibilityDebugger] ✓ Found left hand: {leftHand.name}");
            }
            else
            {
                Debug.LogWarning("[HandVisibilityDebugger] ✗ Left hand not found by name!");

                // Try to find in hierarchy
                GameObject xrOrigin = GameObject.Find("XR Origin (XR Rig)");
                if (xrOrigin != null)
                {
                    Transform leftController = xrOrigin.transform.Find("Camera Offset/Left Controller");
                    if (leftController != null)
                    {
                        Transform hand = leftController.Find("PolytopiaHand_L");
                        if (hand != null)
                        {
                            leftHand = hand.gameObject;
                            Debug.Log("[HandVisibilityDebugger] ✓ Found left hand via hierarchy search");
                        }
                    }
                }
            }

            if (rightHand != null)
            {
                Debug.Log($"[HandVisibilityDebugger] ✓ Found right hand: {rightHand.name}");
            }
            else
            {
                Debug.LogWarning("[HandVisibilityDebugger] ✗ Right hand not found by name!");

                // Try to find in hierarchy
                GameObject xrOrigin = GameObject.Find("XR Origin (XR Rig)");
                if (xrOrigin != null)
                {
                    Transform rightController = xrOrigin.transform.Find("Camera Offset/Right Controller");
                    if (rightController != null)
                    {
                        Transform hand = rightController.Find("PolytopiaHand_R");
                        if (hand != null)
                        {
                            rightHand = hand.gameObject;
                            Debug.Log("[HandVisibilityDebugger] ✓ Found right hand via hierarchy search");
                        }
                    }
                }
            }
        }

        void RunDiagnostics()
        {
            Debug.Log("========================================");
            Debug.Log("[HandVisibilityDebugger] HAND VISIBILITY DIAGNOSTICS");
            Debug.Log("========================================");

            if (leftHand == null && rightHand == null)
            {
                Debug.LogError("[HandVisibilityDebugger] NO HANDS FOUND! Hands might not exist in scene.");
                Debug.Log("Searching all GameObjects for hand-related names...");

                GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
                foreach (GameObject obj in allObjects)
                {
                    if (obj.name.ToLower().Contains("hand") || obj.name.ToLower().Contains("polytopia"))
                    {
                        Debug.Log($"  Found potential hand object: {obj.name} (path: {GetGameObjectPath(obj)})");
                    }
                }
                return;
            }

            if (leftHand != null)
            {
                Debug.Log("--- LEFT HAND ---");
                DiagnoseHand(leftHand, "LEFT");
            }

            if (rightHand != null)
            {
                Debug.Log("--- RIGHT HAND ---");
                DiagnoseHand(rightHand, "RIGHT");
            }

            Debug.Log("========================================");
        }

        void DiagnoseHand(GameObject hand, string handName)
        {
            StringBuilder report = new StringBuilder();
            report.AppendLine($"[{handName} HAND] GameObject: {hand.name}");
            report.AppendLine($"  Active: {hand.activeSelf} (ActiveInHierarchy: {hand.activeInHierarchy})");
            report.AppendLine($"  Layer: {LayerMask.LayerToName(hand.layer)} ({hand.layer})");
            report.AppendLine($"  Position: {hand.transform.position}");
            report.AppendLine($"  LocalPosition: {hand.transform.localPosition}");
            report.AppendLine($"  LocalScale: {hand.transform.localScale}");
            report.AppendLine($"  Rotation: {hand.transform.rotation.eulerAngles}");
            report.AppendLine($"  Hierarchy Path: {GetGameObjectPath(hand)}");

            // Check if in camera view
            if (mainCamera != null)
            {
                Plane[] planes = GeometryUtility.CalculateFrustumPlanes(mainCamera);
                bool inView = false;

                // Check if any child renderer is in view
                MeshRenderer[] renderers = hand.GetComponentsInChildren<MeshRenderer>();
                foreach (MeshRenderer mr in renderers)
                {
                    if (GeometryUtility.TestPlanesAABB(planes, mr.bounds))
                    {
                        inView = true;
                        break;
                    }
                }

                report.AppendLine($"  In Camera View: {inView}");
                report.AppendLine($"  Distance from Camera: {Vector3.Distance(hand.transform.position, mainCamera.transform.position):F2}m");
            }

            // Check all MeshRenderers in hand hierarchy
            MeshRenderer[] allRenderers = hand.GetComponentsInChildren<MeshRenderer>(true);
            report.AppendLine($"  Total MeshRenderers in hierarchy: {allRenderers.Length}");

            int visibleCount = 0;
            int enabledCount = 0;

            foreach (MeshRenderer mr in allRenderers)
            {
                if (mr.enabled) enabledCount++;
                if (mr.enabled && mr.gameObject.activeInHierarchy) visibleCount++;

                report.AppendLine($"    - {mr.gameObject.name}:");
                report.AppendLine($"        Enabled: {mr.enabled}");
                report.AppendLine($"        GameObject Active: {mr.gameObject.activeSelf}");
                report.AppendLine($"        ActiveInHierarchy: {mr.gameObject.activeInHierarchy}");

                if (mr.sharedMaterial != null)
                {
                    report.AppendLine($"        Material: {mr.sharedMaterial.name}");
                    report.AppendLine($"        Shader: {mr.sharedMaterial.shader.name}");
                    report.AppendLine($"        Color: {mr.sharedMaterial.color}");

                    // Check if shader has errors
                    if (!mr.sharedMaterial.shader.isSupported)
                    {
                        report.AppendLine($"        ⚠️ SHADER NOT SUPPORTED!");
                    }
                }
                else
                {
                    report.AppendLine($"        ⚠️ NO MATERIAL!");
                }

                report.AppendLine($"        Bounds: {mr.bounds.center} (size: {mr.bounds.size})");
                report.AppendLine($"        Layer: {LayerMask.LayerToName(mr.gameObject.layer)} ({mr.gameObject.layer})");
            }

            report.AppendLine($"  Enabled Renderers: {enabledCount}/{allRenderers.Length}");
            report.AppendLine($"  Fully Visible Renderers: {visibleCount}/{allRenderers.Length}");

            // Check for components that might hide the hand
            Canvas[] canvases = hand.GetComponentsInChildren<Canvas>(true);
            if (canvases.Length > 0)
            {
                report.AppendLine($"  ⚠️ Found {canvases.Length} Canvas components (might affect rendering)");
            }

            // Check camera culling mask
            if (mainCamera != null)
            {
                int handLayer = hand.layer;
                bool isCulled = (mainCamera.cullingMask & (1 << handLayer)) == 0;
                report.AppendLine($"  Camera Culling Hand Layer: {isCulled}");

                if (isCulled)
                {
                    report.AppendLine($"  ⚠️ WARNING: Hand layer is CULLED by camera! Camera culling mask: {mainCamera.cullingMask}");
                }
            }

            Debug.Log(report.ToString());
        }

        string GetGameObjectPath(GameObject obj)
        {
            string path = obj.name;
            Transform current = obj.transform.parent;
            while (current != null)
            {
                path = current.name + "/" + path;
                current = current.parent;
            }
            return path;
        }

        void OnDrawGizmos()
        {
            if (!drawGizmos) return;

            // Draw hand positions
            if (leftHand != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(leftHand.transform.position, 0.05f);
                Gizmos.DrawLine(leftHand.transform.position, leftHand.transform.position + leftHand.transform.forward * 0.1f);

                // Draw palm
                Transform palm = leftHand.transform.Find("Palm");
                if (palm != null)
                {
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawWireCube(palm.position, Vector3.one * 0.05f);
                }
            }

            if (rightHand != null)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawWireSphere(rightHand.transform.position, 0.05f);
                Gizmos.DrawLine(rightHand.transform.position, rightHand.transform.position + rightHand.transform.forward * 0.1f);

                // Draw palm
                Transform palm = rightHand.transform.Find("Palm");
                if (palm != null)
                {
                    Gizmos.color = Color.cyan;
                    Gizmos.DrawWireCube(palm.position, Vector3.one * 0.05f);
                }
            }

            // Draw camera view
            if (mainCamera != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(mainCamera.transform.position, 0.1f);
                Gizmos.DrawRay(mainCamera.transform.position, mainCamera.transform.forward * 0.5f);
            }
        }
    }
}
